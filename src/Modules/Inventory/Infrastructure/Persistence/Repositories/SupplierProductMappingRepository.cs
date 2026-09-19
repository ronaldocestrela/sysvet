using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Repositories;

public class SupplierProductMappingRepository : ISupplierProductMappingRepository
{
    private readonly InventoryDbContext _dbContext;

    public SupplierProductMappingRepository(InventoryDbContext dbContext) => _dbContext = dbContext;

    public void Add(SupplierProductMapping entity) => _dbContext.SupplierProductMappings.Add(entity);

    public async Task<IEnumerable<SupplierProductMapping>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.SupplierProductMappings.ToListAsync(cancellationToken);

    public async Task<SupplierProductMapping?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.SupplierProductMappings.FindAsync([id], cancellationToken);

    public void Remove(SupplierProductMapping entity) => _dbContext.SupplierProductMappings.Remove(entity);

    public void Update(SupplierProductMapping entity) => _dbContext.SupplierProductMappings.Update(entity);

    public async Task<SupplierProductMapping?> GetBySupplierAndCodeAsync(
        Guid supplierId,
        string supplierProductCode,
        CancellationToken cancellationToken = default)
        => await _dbContext.SupplierProductMappings.FirstOrDefaultAsync(
            m => m.SupplierId == supplierId && m.SupplierProductCode == supplierProductCode.Trim(),
            cancellationToken);
}
