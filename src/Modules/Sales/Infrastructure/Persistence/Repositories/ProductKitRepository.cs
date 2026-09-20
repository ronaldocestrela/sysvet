using Microsoft.EntityFrameworkCore;
using Sales.Domain.Entities;
using Sales.Domain.Repositories;

namespace Sales.Infrastructure.Persistence.Repositories;

public sealed class ProductKitRepository : IProductKitRepository
{
    private readonly SalesDbContext _dbContext;

    public ProductKitRepository(SalesDbContext dbContext) => _dbContext = dbContext;

    public async Task<ProductKit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.ProductKits.Include(k => k.Components).FirstOrDefaultAsync(k => k.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ProductKit>> ListAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.ProductKits.Include(k => k.Components).AsNoTracking().ToListAsync(cancellationToken);

    public void Add(ProductKit kit) => _dbContext.ProductKits.Add(kit);

    public void Update(ProductKit kit) => _dbContext.ProductKits.Update(kit);
}
