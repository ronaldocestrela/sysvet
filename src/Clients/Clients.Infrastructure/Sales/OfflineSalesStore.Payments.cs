using Clients.Infrastructure.Http;
using Core.Domain;
using Microsoft.EntityFrameworkCore;
using Sales.Domain.Entities;
using Sales.Domain.Enums;
using Sales.Domain.Payments;
using Sales.Domain.Queries;

namespace Clients.Infrastructure.Sales;

public sealed partial class OfflineSalesStore
{
    private async Task<Result<List<Payment>>> BuildPaymentsAsync(
        IReadOnlyList<PayOrderPaymentClientDto> payments,
        CancellationToken cancellationToken)
    {
        var entities = new List<Payment>();
        var authorized = new List<PaymentTerminalRefundRequest>();

        foreach (var p in payments)
        {
            if (!Enum.TryParse<PaymentMethod>(p.Method, true, out var method))
            {
                method = PaymentMethod.Cash;
            }

            var nsu = p.Nsu;
            var authorizationCode = p.AuthorizationCode;
            var provider = p.Provider;
            var installments = p.Installments;

            if (Payment.RequiresTefNsu(method) && string.IsNullOrWhiteSpace(nsu))
            {
                var authResult = await _paymentTerminal.AuthorizeAsync(
                    new PaymentTerminalAuthorizeRequest(method, p.Amount, installments),
                    cancellationToken);
                if (authResult.IsFailure)
                {
                    await CompensateAsync(authorized, cancellationToken);
                    return Result.Failure<List<Payment>>(authResult.Error);
                }

                var auth = authResult.Value;
                nsu = auth.Nsu;
                authorizationCode = auth.AuthorizationCode;
                provider = auth.Provider;
                installments = auth.Installments;
                authorized.Add(new PaymentTerminalRefundRequest(method, p.Amount, auth.Nsu));
            }

            var created = Payment.Create(
                method,
                p.Amount,
                nsu,
                authorizationCode,
                provider,
                installments: installments);
            if (created.IsFailure)
            {
                await CompensateAsync(authorized, cancellationToken);
                return Result.Failure<List<Payment>>(created.Error);
            }

            entities.Add(created.Value);
        }

        return Result.Success(entities);
    }

    private async Task CompensateAsync(
        IReadOnlyList<PaymentTerminalRefundRequest> authorized,
        CancellationToken cancellationToken)
    {
        foreach (var item in authorized)
        {
            await _paymentTerminal.RefundAsync(item, cancellationToken);
        }
    }

    private async Task<IReadOnlyList<CashRegisterPaymentTotals>> GetPaymentTotalsForRegisterAsync(
        Guid cashRegisterId,
        CancellationToken cancellationToken)
    {
        var paidStatuses = new[]
        {
            OrderStatus.Paid,
            OrderStatus.PartiallyRefunded,
            OrderStatus.Refunded
        };

        var payments = await _dbContext.Payments
            .AsNoTracking()
            .Where(p => _dbContext.Orders.Any(o =>
                o.Id == p.OrderId &&
                o.CashRegisterId == cashRegisterId &&
                paidStatuses.Contains(o.Status)))
            .Include(p => p.Refunds)
            .ToListAsync(cancellationToken);

        return payments
            .GroupBy(p => p.Method)
            .Select(g => new CashRegisterPaymentTotals
            {
                Method = g.Key,
                Gross = g.Sum(p => p.Amount.Amount),
                Refunded = g.SelectMany(p => p.Refunds).Sum(r => r.Amount.Amount)
            })
            .ToList();
    }

    private static object ToPayOutboxPayment(Payment payment) => new
    {
        Method = payment.Method,
        Amount = payment.Amount.Amount,
        payment.Nsu,
        payment.AuthorizationCode,
        payment.Provider,
        payment.TerminalId,
        payment.Brand,
        payment.Installments
    };
}
