using Clients.Infrastructure.Http;
using Clients.Infrastructure.Sync;
using Core.Domain;
using Microsoft.EntityFrameworkCore;
using Sales.Domain.Entities;
using Sales.Domain.Enums;
using Sales.Domain.Payments;
using SalesErrorCodes = Sales.Domain.ErrorCodes;

namespace Clients.Infrastructure.Sales;

/// <summary>SQLite-backed PDV with transactional outbox (ADR-026).</summary>
public sealed partial class OfflineSalesStore : ISalesStore
{
    private readonly OfflineDbContext _dbContext;
    private readonly SyncWakeSignal _wakeSignal;
    private readonly ISyncConnectivity _connectivity;
    private readonly IPaymentTerminal _paymentTerminal;

    public OfflineSalesStore(
        OfflineDbContext dbContext,
        SyncWakeSignal wakeSignal,
        ISyncConnectivity connectivity,
        IPaymentTerminal paymentTerminal)
    {
        _dbContext = dbContext;
        _wakeSignal = wakeSignal;
        _connectivity = connectivity;
        _paymentTerminal = paymentTerminal;
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

        var totals = await GetPaymentTotalsForRegisterAsync(register.Id, cancellationToken);
        var cashRow = totals.FirstOrDefault(t => t.Method == PaymentMethod.Cash);
        var cashNet = cashRow is null ? 0m : cashRow.Gross - cashRow.Refunded;

        return Result.Success<CashRegisterClientDto?>(new CashRegisterClientDto
        {
            Id = register.Id,
            Status = register.Status.ToString(),
            OpeningBalance = register.OpeningBalance.Amount,
            CurrentBalance = register.OpeningBalance.Amount + cashNet,
            MethodTotals = totals.Select(t => new CashRegisterMethodTotalsClientDto
            {
                Method = t.Method.ToString(),
                Gross = t.Gross,
                Refunded = t.Refunded
            }).ToList()
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

        var buildPayments = await BuildPaymentsAsync(payments, cancellationToken);
        if (buildPayments.IsFailure)
        {
            return Result.Failure<Guid>(buildPayments.Error);
        }

        var paymentEntities = buildPayments.Value;
        var authorizedForCompensation = paymentEntities
            .Where(p => Payment.RequiresTefNsu(p.Method) && !string.IsNullOrWhiteSpace(p.Nsu))
            .Select(p => new PaymentTerminalRefundRequest(p.Method, p.Amount.Amount, p.Nsu!))
            .ToList();

        var stockLines = order.Items
            .Where(i => i.Kind == OrderItemKind.Product && i.ProductId.HasValue)
            .Select(i => (i.ProductId!.Value, i.Quantity))
            .ToList();

        if (stockLines.Count > 0)
        {
            var debit = await OfflineLocalStockSaleDebiter.DebitAsync(_dbContext, stockLines, cancellationToken);
            if (debit.IsFailure)
            {
                await CompensateAsync(authorizedForCompensation, cancellationToken);
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

        var paymentPayloads = paymentEntities.Select(p => (object)ToPayOutboxPayment(p)).ToList();

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
    public async Task<Result<Guid>> RefundOrderPaymentAsync(
        Guid orderId,
        Guid paymentId,
        decimal amount,
        string? refundNsu = null,
        CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Payments)
            .ThenInclude(p => p.Refunds)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<Guid>(SalesErrorCodes.Order.NotFound);
        }

        var payment = order.Payments.FirstOrDefault(p => p.Id == paymentId);
        if (payment is null)
        {
            return Result.Failure<Guid>(SalesErrorCodes.Payment.NotFound);
        }

        if (Payment.RequiresTefNsu(payment.Method) && string.IsNullOrWhiteSpace(refundNsu))
        {
            if (string.IsNullOrWhiteSpace(payment.Nsu))
            {
                return Result.Failure<Guid>(SalesErrorCodes.Payment.NsuRequired);
            }

            var terminalResult = await _paymentTerminal.RefundAsync(
                new PaymentTerminalRefundRequest(payment.Method, amount, payment.Nsu),
                cancellationToken);
            if (terminalResult.IsFailure)
            {
                return Result.Failure<Guid>(terminalResult.Error);
            }

            refundNsu = terminalResult.Value.RefundNsu;
        }

        var refundResult = order.RefundPayment(paymentId, amount, refundNsu);
        if (refundResult.IsFailure)
        {
            return Result.Failure<Guid>(refundResult.Error);
        }

        var outboxId = Guid.NewGuid();
        EnqueueOutbox(
            "RefundOrderPaymentCommand",
            OutboxPayloadFactory.RefundOrderPayment(orderId, paymentId, amount, refundNsu, outboxId),
            outboxId);

        await _dbContext.SaveChangesAsync(cancellationToken);
        RequestSyncIfOnline();
        return Result.Success(refundResult.Value.Id);
    }

    /// <inheritdoc />
    public async Task<Result<SalesOrderDetailClientDto>> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders.AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .ThenInclude(p => p.Refunds)
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
            .Where(m => m.Type == "CreateOrderCommand" || m.Type == "PayOrderCommand" || m.Type == "RefundOrderPaymentCommand")
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
                Id = p.Id,
                Method = p.Method.ToString(),
                Amount = p.Amount.Amount,
                Nsu = p.Nsu,
                AuthorizationCode = p.AuthorizationCode,
                Provider = p.Provider,
                Installments = p.Installments,
                RemainingRefundable = p.RemainingRefundable,
                Refunds = p.Refunds.Select(r => new SalesOrderPaymentRefundClientDto
                {
                    Id = r.Id,
                    Amount = r.Amount.Amount,
                    RefundNsu = r.RefundNsu,
                    CreatedAt = r.CreatedAt
                }).ToList()
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
