using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Sales.Domain.Entities;
using Sales.Domain.Enums;
using Sales.Domain.Repositories;

namespace Sales.Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
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
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> SumCashPaymentsForCashRegisterAsync(Guid cashRegisterId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Payments
            .Where(p => p.Method == PaymentMethod.Cash)
            .Join(
                _dbContext.Orders.Where(o => o.CashRegisterId == cashRegisterId && o.Status == OrderStatus.Paid),
                p => p.OrderId,
                o => o.Id,
                (p, _) => p.Amount.Amount)
            .SumAsync(cancellationToken);
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
        }
    }

    public void Remove(Order order)
    {
        _dbContext.Orders.Remove(order);
    }
}
