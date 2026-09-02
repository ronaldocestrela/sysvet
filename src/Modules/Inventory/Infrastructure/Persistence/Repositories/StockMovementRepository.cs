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
}
