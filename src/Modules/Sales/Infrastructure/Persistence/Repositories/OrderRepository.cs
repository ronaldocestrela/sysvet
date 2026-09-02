using Microsoft.EntityFrameworkCore;
using Sales.Domain.Entities;
using Sales.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders.Include(o => o.Items).ToListAsync(cancellationToken);
    }

    public void Add(Order order)
    {
        _dbContext.Orders.Add(order);
    }

    public void Update(Order order)
    {
        _dbContext.Orders.Update(order);
    }

    public void Remove(Order order)
    {
        _dbContext.Orders.Remove(order);
    }
}
