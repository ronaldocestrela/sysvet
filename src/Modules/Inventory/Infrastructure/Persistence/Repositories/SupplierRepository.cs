using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Repositories;

public class SupplierRepository : ISupplierRepository
{
    private readonly InventoryDbContext _dbContext;

    public SupplierRepository(InventoryDbContext dbContext) => _dbContext = dbContext;

    public void Add(Supplier entity) => _dbContext.Suppliers.Add(entity);

    public async Task<IEnumerable<Supplier>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Suppliers.OrderBy(s => s.TradeName).ToListAsync(cancellationToken);

    public async Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.Suppliers.FindAsync([id], cancellationToken);

    public void Remove(Supplier entity) => _dbContext.Suppliers.Remove(entity);

    public void Update(Supplier entity) => _dbContext.Suppliers.Update(entity);

    public async Task<Supplier?> GetByDocumentAsync(string document, CancellationToken cancellationToken = default)
    {
        var digits = new string(document.Where(char.IsDigit).ToArray());
        return await _dbContext.Suppliers.FirstOrDefaultAsync(s => s.Document == digits, cancellationToken);
    }
}
