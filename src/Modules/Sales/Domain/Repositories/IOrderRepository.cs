using Core.Domain;
using Sales.Domain.Entities;
using Sales.Domain.Enums;
using Sales.Domain.Queries;

namespace Sales.Domain.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    /// <summary>
    /// Aggregates gross and refunded amounts per payment method for orders in paid/refunded states.
    /// </summary>
    Task<IReadOnlyList<CashRegisterPaymentTotals>> GetPaymentTotalsForCashRegisterAsync(
        Guid cashRegisterId,
        CancellationToken cancellationToken = default);

    /// <summary>Persists a refund row and order status without reloading the aggregate (avoids RowVersion conflicts).</summary>
    Task PersistRefundAsync(
        Guid orderId,
        PaymentRefund refund,
        OrderStatus status,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken = default);
}
