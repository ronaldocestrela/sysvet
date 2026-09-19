using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Repositories;

public class InventoryCountRepository : IInventoryCountRepository
{
    private readonly InventoryDbContext _dbContext;

    public InventoryCountRepository(InventoryDbContext dbContext) => _dbContext = dbContext;

    public void Add(InventoryCount entity) => _dbContext.InventoryCounts.Add(entity);

    public async Task<IEnumerable<InventoryCount>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.InventoryCounts.ToListAsync(cancellationToken);

    public async Task<InventoryCount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.InventoryCounts.FindAsync([id], cancellationToken);

    public void Remove(InventoryCount entity) => _dbContext.InventoryCounts.Remove(entity);

    public void Update(InventoryCount entity) => _dbContext.InventoryCounts.Update(entity);

    public async Task<InventoryCount?> GetByIdWithLinesAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.InventoryCounts
            .Include(c => c.Lines)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<InventoryCount?> GetInProgressAsync(CancellationToken cancellationToken = default)
        => await _dbContext.InventoryCounts
            .Include(c => c.Lines)
            .FirstOrDefaultAsync(c => c.Status == InventoryCountStatus.InProgress, cancellationToken);

    public async Task<IReadOnlyList<InventoryCount>> ListRecentAsync(int take, CancellationToken cancellationToken = default)
        => await _dbContext.InventoryCounts
            .Include(c => c.Lines)
            .OrderByDescending(c => c.UpdatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

    public void StageLine(InventoryCountLine line) => _dbContext.InventoryCountLines.Add(line);
}
