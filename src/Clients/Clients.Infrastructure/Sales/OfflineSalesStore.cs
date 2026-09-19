using Clients.Infrastructure.Http;
using Clients.Infrastructure.Sync;
using Core.Domain;
using Microsoft.EntityFrameworkCore;
using Sales.Domain.Entities;
using Sales.Domain.Enums;
using SalesErrorCodes = Sales.Domain.ErrorCodes;

namespace Clients.Infrastructure.Sales;

/// <summary>SQLite-backed PDV with transactional outbox (ADR-026).</summary>
public sealed class OfflineSalesStore : ISalesStore
{
    private readonly OfflineDbContext _dbContext;
    private readonly SyncWakeSignal _wakeSignal;
    private readonly ISyncConnectivity _connectivity;

    public OfflineSalesStore(OfflineDbContext dbContext, SyncWakeSignal wakeSignal, ISyncConnectivity connectivity)
    {
        _dbContext = dbContext;
        _wakeSignal = wakeSignal;
        _connectivity = connectivity;
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> OpenCashRegisterAsync(decimal openingBalance, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.CashRegisters.FirstOrDefaultAsync(
            c => c.Status == CashRegisterStatus.Open,
            cancellationToken);
        if (existing is not null)
        {
            return Result.Failure<Guid>(SalesErrorCodes.CashRegister.AlreadyOpen);
        }

        var registerId = Guid.NewGuid();
        var opened = CashRegister.Open(registerId, Guid.Empty, openingBalance);
        if (opened.IsFailure)
        {
            return Result.Failure<Guid>(opened.Error);
        }

        _dbContext.CashRegisters.Add(opened.Value);
        var outboxId = Guid.NewGuid();
        EnqueueOutbox("OpenCashRegisterCommand", OutboxPayloadFactory.OpenCashRegister(registerId, openingBalance, outboxId), outboxId);
        await _dbContext.SaveChangesAsync(cancellationToken);
        RequestSyncIfOnline();
        return Result.Success(registerId);
    }

    /// <inheritdoc />
    public async Task<Result<bool>> CloseCashRegisterAsync(Guid cashRegisterId, decimal actualClosingBalance, CancellationToken cancellationToken = default)
    {
        var register = await _dbContext.CashRegisters.FirstOrDefaultAsync(c => c.Id == cashRegisterId, cancellationToken);
        if (register is null)
        {
            return Result.Failure<bool>(SalesErrorCodes.CashRegister.NotFound);
        }

        if (register.Status == CashRegisterStatus.Closed)
        {
            return Result.Success(true);
        }

        var closed = register.Close(actualClosingBalance);
        if (closed.IsFailure)
        {
            return closed;
        }

        var outboxId = Guid.NewGuid();
        EnqueueOutbox("CloseCashRegisterCommand", OutboxPayloadFactory.CloseCashRegister(cashRegisterId, actualClosingBalance, outboxId), outboxId);
        await _dbContext.SaveChangesAsync(cancellationToken);
        RequestSyncIfOnline();
        return Result.Success(true);
    }

    /// <inheritdoc />
    public async Task<Result<CashRegisterClientDto?>> GetOpenCashRegisterAsync(CancellationToken cancellationToken = default)
    {
        var register = await _dbContext.CashRegisters.FirstOrDefaultAsync(
            c => c.Status == CashRegisterStatus.Open,
            cancellationToken);
        if (register is null)
        {
            return Result.Success<CashRegisterClientDto?>(null);
        }

        var cashSales = await SumCashPaymentsForRegisterAsync(register.Id, cancellationToken);
        return Result.Success<CashRegisterClientDto?>(new CashRegisterClientDto
        {
            Id = register.Id,
            Status = register.Status.ToString(),
            OpeningBalance = register.OpeningBalance.Amount,
            CurrentBalance = register.OpeningBalance.Amount + cashSales
        });
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> CreateAndPayOrderAsync(
        CreateSalesOrderClientRequest request,
        IReadOnlyList<PayOrderPaymentClientDto> payments,
        CancellationToken cancellationToken = default)
    {
        var register = await _dbContext.CashRegisters.FirstOrDefaultAsync(
            c => c.Id == request.CashRegisterId && c.Status == CashRegisterStatus.Open,
            cancellationToken);
        if (register is null)
        {
            return Result.Failure<Guid>(SalesErrorCodes.Order.CashRegisterNotOpen);
        }

        var orderId = Guid.NewGuid();
        var orderResult = Order.Create(orderId, request.CashRegisterId, request.TutorId, request.PetId, request.SourceQuoteId);
        if (orderResult.IsFailure)
        {
            return Result.Failure<Guid>(orderResult.Error);
        }

        var order = orderResult.Value;
        foreach (var item in request.Items)
        {
            var kind = string.Equals(item.Kind, "Service", StringComparison.OrdinalIgnoreCase)
                ? OrderItemKind.Service
                : OrderItemKind.Product;
            var add = kind == OrderItemKind.Service
                ? order.AddServiceItem(item.ProductName, item.Quantity, item.UnitPrice)
                : order.AddProductItem(item.ProductId ?? Guid.Empty, item.ProductName, item.Quantity, item.UnitPrice);
            if (add.IsFailure)
            {
                return Result.Failure<Guid>(add.Error);
            }
        }

        var paymentEntities = new List<Payment>();
        foreach (var p in payments)
        {
            if (!Enum.TryParse<PaymentMethod>(p.Method, true, out var method))
            {
                method = PaymentMethod.Cash;
            }

            var created = Payment.Create(method, p.Amount);
            if (created.IsFailure)
            {
                return Result.Failure<Guid>(created.Error);
            }

            paymentEntities.Add(created.Value);
        }

        var stockLines = order.Items
            .Where(i => i.Kind == OrderItemKind.Product && i.ProductId.HasValue)
            .Select(i => (i.ProductId!.Value, i.Quantity))
            .ToList();

        if (stockLines.Count > 0)
        {
            var debit = await OfflineLocalStockSaleDebiter.DebitAsync(_dbContext, stockLines, cancellationToken);
            if (debit.IsFailure)
            {
                return Result.Failure<Guid>(SalesErrorCodes.Order.InsufficientStock);
            }
        }

        var payResult = order.Pay(paymentEntities);
        if (payResult.IsFailure)
        {
            return Result.Failure<Guid>(payResult.Error);
        }

        _dbContext.Orders.Add(order);

        var itemPayloads = request.Items.Select(i => (object)new
        {
            Kind = string.Equals(i.Kind, "Service", StringComparison.OrdinalIgnoreCase) ? OrderItemKind.Service : OrderItemKind.Product,
            i.ProductId,
            i.ProductName,
            i.Quantity,
            i.UnitPrice
        }).ToList();

        var createOutboxId = Guid.NewGuid();
        EnqueueOutbox(
            "CreateOrderCommand",
            OutboxPayloadFactory.CreateOrder(
                orderId,
                request.CashRegisterId,
                request.TutorId,
                request.PetId,
                request.SourceQuoteId,
                itemPayloads,
                createOutboxId),
            createOutboxId);

        var paymentPayloads = payments.Select(p => (object)new
        {
            Method = Enum.TryParse<PaymentMethod>(p.Method, true, out var m) ? m : PaymentMethod.Cash,
            p.Amount
        }).ToList();

        var payOutboxId = Guid.NewGuid();
        EnqueueOutbox(
            "PayOrderCommand",
            OutboxPayloadFactory.PayOrder(orderId, paymentPayloads, payOutboxId),
            payOutboxId);

        await _dbContext.SaveChangesAsync(cancellationToken);
        RequestSyncIfOnline();
        return Result.Success(orderId);
    }

    /// <inheritdoc />
    public async Task<Result<SalesOrderDetailClientDto>> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders.AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure<SalesOrderDetailClientDto>(SalesErrorCodes.Order.NotFound);
        }

        return Result.Success(MapDetail(order));
    }

    /// <inheritdoc />
    public async Task<SalesOrderSyncState> GetOrderSyncStateAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var orderIdText = orderId.ToString();
        var messages = await _dbContext.OutboxMessages
            .Where(m => m.Type == "CreateOrderCommand" || m.Type == "PayOrderCommand")
            .ToListAsync(cancellationToken);

        var related = messages.Where(m => m.Payload.Contains(orderIdText, StringComparison.OrdinalIgnoreCase)).ToList();
        if (related.Any(m => m.Error != null))
        {
            return SalesOrderSyncState.Conflict;
        }

        if (related.Any(m => m.ProcessedAt == null && m.Error == null))
        {
            return SalesOrderSyncState.Pending;
        }

        return related.Count == 0 ? SalesOrderSyncState.Synced : SalesOrderSyncState.Synced;
    }

    private async Task<decimal> SumCashPaymentsForRegisterAsync(Guid cashRegisterId, CancellationToken cancellationToken)
    {
        var paidOrderIds = await _dbContext.Orders
            .Where(o => o.CashRegisterId == cashRegisterId && o.Status == OrderStatus.Paid)
            .Select(o => o.Id)
            .ToListAsync(cancellationToken);

        if (paidOrderIds.Count == 0)
        {
            return 0m;
        }

        return await _dbContext.Payments
            .Where(p => paidOrderIds.Contains(p.OrderId) && p.Method == PaymentMethod.Cash)
            .SumAsync(p => p.Amount.Amount, cancellationToken);
    }

    private static SalesOrderDetailClientDto MapDetail(Order order) =>
        new()
        {
            Id = order.Id,
            Status = order.Status.ToString(),
            TutorId = order.TutorId,
            PetId = order.PetId,
            FinanceIntegrationStatus = order.FinanceIntegrationStatus.ToString(),
            CreatedAt = order.CreatedAt,
            PaidAt = order.PaidAt,
            TotalAmount = order.TotalAmount.Amount,
            Items = order.Items.Select(i => new SalesOrderItemClientDto
            {
                Kind = i.Kind.ToString(),
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice.Amount
            }).ToList(),
            Payments = order.Payments.Select(p => new PayOrderPaymentClientDto
            {
                Method = p.Method.ToString(),
                Amount = p.Amount.Amount
            }).ToList()
        };

    private void EnqueueOutbox(string type, string payload, Guid id)
    {
        var createdAt = DateTimeOffset.UtcNow;
        var latestInBatch = _dbContext.OutboxMessages.Local
            .Select(m => m.CreatedAt)
            .DefaultIfEmpty(DateTimeOffset.MinValue)
            .Max();
        if (createdAt <= latestInBatch)
        {
            createdAt = latestInBatch.AddTicks(1);
        }

        _dbContext.OutboxMessages.Add(new OutboxMessage
        {
            Id = id,
            Type = type,
            Payload = payload,
            CreatedAt = createdAt
        });
    }

    private void RequestSyncIfOnline()
    {
        if (_connectivity.IsOnline)
        {
            _wakeSignal.RequestSync();
        }
    }
}
