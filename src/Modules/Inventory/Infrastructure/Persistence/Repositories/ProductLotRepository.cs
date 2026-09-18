using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Repositories;

public class ProductLotRepository : IProductLotRepository
{
    private readonly InventoryDbContext _dbContext;

    public ProductLotRepository(InventoryDbContext dbContext) => _dbContext = dbContext;

    public void Add(ProductLot entity) => _dbContext.ProductLots.Add(entity);

    public async Task<IEnumerable<ProductLot>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.ProductLots.ToListAsync(cancellationToken);

    public async Task<ProductLot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.ProductLots.FindAsync([id], cancellationToken);

    public void Remove(ProductLot entity) => _dbContext.ProductLots.Remove(entity);

    public void Update(ProductLot entity) => _dbContext.ProductLots.Update(entity);

    public async Task<IReadOnlyList<ProductLot>> ListByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
        => await _dbContext.ProductLots.Where(l => l.ProductId == productId).OrderBy(l => l.LotNumber).ToListAsync(cancellationToken);

    public async Task<ProductLot?> GetByProductAndLotNumberAsync(Guid productId, string lotNumber, CancellationToken cancellationToken = default)
    {
        var normalized = lotNumber.Trim().ToUpperInvariant();
        return await _dbContext.ProductLots.FirstOrDefaultAsync(
            l => l.ProductId == productId && l.LotNumber == normalized,
            cancellationToken);
    }
}
