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
        OrderStatus.Refunded,
        OrderStatus.PartiallyReturned,
        OrderStatus.Returned
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
            .Include(o => o.Commissions)
            .Include(o => o.Returns)
            .ThenInclude(r => r.Lines)
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
        if (_dbContext.Entry(order).State == EntityState.Detached)
        {
            _dbContext.Orders.Attach(order);
        }

        foreach (var payment in order.Payments)
        {
            AddIfDetached(payment);
            foreach (var refund in payment.Refunds)
            {
                AddIfDetached(refund);
            }
        }

        foreach (var commission in order.Commissions)
        {
            AddIfDetached(commission);
        }

        foreach (var saleReturn in order.Returns)
        {
            AddIfDetached(saleReturn);
            foreach (var line in saleReturn.Lines)
            {
                AddIfDetached(line);
            }
        }
    }

    private void AddIfDetached<TEntity>(TEntity entity)
        where TEntity : class
    {
        if (_dbContext.Entry(entity).State == EntityState.Detached)
        {
            _dbContext.Add(entity);
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

    /// <inheritdoc />
    public async Task<IReadOnlyList<Order>> ListByTutorIdAsync(Guid tutorId, CancellationToken cancellationToken = default) =>
        await _dbContext.Orders
            .Where(o => o.TutorId == tutorId)
            .ToListAsync(cancellationToken);
}
