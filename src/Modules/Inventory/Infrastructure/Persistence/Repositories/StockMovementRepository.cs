using System;
using System.Threading;
using System.Threading.Tasks;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Repositories;

public class StockMovementRepository : IStockMovementRepository
{
    private readonly InventoryDbContext _dbContext;

    public StockMovementRepository(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(StockMovement entity) => _dbContext.StockMovements.Add(entity);

    public async Task<System.Collections.Generic.IEnumerable<StockMovement>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.StockMovements.ToListAsync(cancellationToken);

    public async Task<StockMovement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.StockMovements.FindAsync(new object[] { id }, cancellationToken);

    public void Remove(StockMovement entity) => _dbContext.StockMovements.Remove(entity);

    public void Update(StockMovement entity) => _dbContext.StockMovements.Update(entity);

    public async Task<IReadOnlyList<StockMovement>> ListByProductAsync(Guid productId, CancellationToken cancellationToken = default)
        => await _dbContext.StockMovements
            .AsNoTracking()
            .Where(m => m.ProductId == productId)
            .OrderBy(m => m.Date)
            .ThenBy(m => m.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<StockMovement>> ListRecentAsync(Guid? productId, string? reason, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.StockMovements.AsNoTracking().AsQueryable();
        if (productId is Guid pid)
        {
            query = query.Where(m => m.ProductId == pid);
        }

        if (!string.IsNullOrWhiteSpace(reason))
        {
            query = query.Where(m => m.Reason == reason);
        }

        return await query
            .OrderByDescending(m => m.Date)
            .ThenByDescending(m => m.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
