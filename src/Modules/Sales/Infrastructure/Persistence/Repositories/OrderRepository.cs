using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Sales.Domain.Entities;
using Sales.Domain.Enums;
using Sales.Domain.Queries;
using Sales.Domain.Repositories;

namespace Sales.Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private static readonly OrderStatus[] PaidSessionStatuses =
    [
        OrderStatus.Paid,
        OrderStatus.PartiallyRefunded,
        OrderStatus.Refunded
    ];

    private readonly SalesDbContext _dbContext;

    public OrderRepository(SalesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .ThenInclude(p => p.Refunds)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .ThenInclude(p => p.Refunds)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CashRegisterPaymentTotals>> GetPaymentTotalsForCashRegisterAsync(
        Guid cashRegisterId,
        CancellationToken cancellationToken = default)
    {
        var payments = await _dbContext.Payments
            .AsNoTracking()
            .Where(p => _dbContext.Orders.Any(o =>
                o.Id == p.OrderId &&
                o.CashRegisterId == cashRegisterId &&
                PaidSessionStatuses.Contains(o.Status)))
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

    public void Add(Order order)
    {
        _dbContext.Orders.Add(order);
    }

    public void Update(Order order)
    {
        _dbContext.Orders.Update(order);
        foreach (var payment in order.Payments)
        {
            EntityEntry<Payment> entry = _dbContext.Entry(payment);
            if (entry.State == EntityState.Detached)
            {
                _dbContext.Payments.Add(payment);
            }

            foreach (var refund in payment.Refunds)
            {
                EntityEntry<PaymentRefund> refundEntry = _dbContext.Entry(refund);
                if (refundEntry.State == EntityState.Detached)
                {
                    _dbContext.PaymentRefunds.Add(refund);
                }
            }
        }
    }

    /// <inheritdoc cref="IOrderRepository.PersistRefundAsync"/>
    public async Task PersistRefundAsync(
        Guid orderId,
        PaymentRefund refund,
        OrderStatus status,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken = default)
    {
        _dbContext.PaymentRefunds.Add(refund);
        await _dbContext.Orders
            .Where(o => o.Id == orderId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(o => o.Status, status)
                    .SetProperty(o => o.UpdatedAt, updatedAt),
                cancellationToken);
    }

    public void Remove(Order order)
    {
        _dbContext.Orders.Remove(order);
    }
}
