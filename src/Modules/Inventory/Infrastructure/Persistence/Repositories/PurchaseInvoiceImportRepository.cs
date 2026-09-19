using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Repositories;

public class PurchaseInvoiceImportRepository : IPurchaseInvoiceImportRepository
{
    private readonly InventoryDbContext _dbContext;

    public PurchaseInvoiceImportRepository(InventoryDbContext dbContext) => _dbContext = dbContext;

    public void Add(PurchaseInvoiceImport entity) => _dbContext.PurchaseInvoiceImports.Add(entity);

    public async Task<IEnumerable<PurchaseInvoiceImport>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.PurchaseInvoiceImports.ToListAsync(cancellationToken);

    public async Task<PurchaseInvoiceImport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.PurchaseInvoiceImports.FindAsync([id], cancellationToken);

    public void Remove(PurchaseInvoiceImport entity) => _dbContext.PurchaseInvoiceImports.Remove(entity);

    public void Update(PurchaseInvoiceImport entity) => _dbContext.PurchaseInvoiceImports.Update(entity);

    public async Task<PurchaseInvoiceImport?> GetByIdWithLinesAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.PurchaseInvoiceImports
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task<PurchaseInvoiceImport?> GetByAccessKeyAsync(string accessKey, CancellationToken cancellationToken = default)
        => await _dbContext.PurchaseInvoiceImports
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.AccessKey == accessKey, cancellationToken);

    public async Task<IReadOnlyList<PurchaseInvoiceImport>> ListRecentAsync(int take, CancellationToken cancellationToken = default)
        => await _dbContext.PurchaseInvoiceImports
            .OrderByDescending(i => i.IssuedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
}
