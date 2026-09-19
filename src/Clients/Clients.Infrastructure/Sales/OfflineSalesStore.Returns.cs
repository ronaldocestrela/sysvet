using Core.Domain;
using Microsoft.EntityFrameworkCore;
using Sales.Domain.Entities;
using Sales.Domain.Enums;
using Sales.Domain.Payments;
using SalesErrorCodes = Sales.Domain.ErrorCodes;

namespace Clients.Infrastructure.Sales;

public sealed partial class OfflineSalesStore
{
    /// <inheritdoc />
    public async Task<Result<Guid>> ReturnOrderAsync(
        Guid orderId,
        Guid returnId,
        IReadOnlyList<ReturnOrderLineClientDto> lines,
        CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .ThenInclude(p => p.Refunds)
            .Include(o => o.Commissions)
            .Include(o => o.Returns)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<Guid>(SalesErrorCodes.Order.NotFound);
        }

        if (order.Returns.Any(r => r.Id == returnId))
        {
            return Result.Success(returnId);
        }

        var stockLines = lines
            .Select(l =>
            {
                var item = order.Items.FirstOrDefault(i => i.Id == l.OrderItemId);
                return item is { Kind: OrderItemKind.Product, ProductId: not null }
                    ? (item.ProductId.Value, l.Quantity)
                    : ((Guid, decimal)?)null;
            })
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToList();

        if (stockLines.Count > 0)
        {
            var credit = await OfflineLocalStockSaleDebiter.CreditAsync(_dbContext, stockLines, cancellationToken);
            if (credit.IsFailure)
            {
                return Result.Failure<Guid>(credit.Error);
            }
        }

        var returnResult = order.ReturnItems(returnId, lines.Select(l => (l.OrderItemId, l.Quantity)).ToList());
        if (returnResult.IsFailure)
        {
            return Result.Failure<Guid>(returnResult.Error);
        }

        var refundRemaining = returnResult.Value.RefundAmount.Amount;
        foreach (var payment in order.Payments.OrderByDescending(p => p.Method == PaymentMethod.Cash))
        {
            if (refundRemaining <= 0)
            {
                break;
            }

            var slice = Math.Min(refundRemaining, payment.RemainingRefundable);
            if (slice <= 0)
            {
                continue;
            }

            string? refundNsu = null;
            if (Payment.RequiresTefNsu(payment.Method))
            {
                if (string.IsNullOrWhiteSpace(payment.Nsu))
                {
                    return Result.Failure<Guid>(SalesErrorCodes.Payment.NsuRequired);
                }

                var terminalResult = await _paymentTerminal.RefundAsync(
                    new PaymentTerminalRefundRequest(payment.Method, slice, payment.Nsu),
                    cancellationToken);
                if (terminalResult.IsFailure)
                {
                    return Result.Failure<Guid>(terminalResult.Error);
                }

                refundNsu = terminalResult.Value.RefundNsu;
            }

            var refund = order.RefundPayment(payment.Id, slice, refundNsu);
            if (refund.IsFailure)
            {
                return Result.Failure<Guid>(refund.Error);
            }

            refundRemaining -= slice;
        }

        var outboxId = Guid.NewGuid();
        EnqueueOutbox(
            "ReturnOrderCommand",
            System.Text.Json.JsonSerializer.Serialize(new
            {
                OrderId = orderId,
                ReturnId = returnId,
                Lines = lines.Select(l => new { l.OrderItemId, l.Quantity }),
                IdempotencyKey = outboxId
            }),
            outboxId);

        await _dbContext.SaveChangesAsync(cancellationToken);
        RequestSyncIfOnline();
        return Result.Success(returnId);
    }
}
