using System;
using System.Threading;
using System.Threading.Tasks;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly InventoryDbContext _dbContext;

    public ProductRepository(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(Product entity) => _dbContext.Products.Add(entity);

    public async Task AddAsync(Product entity, CancellationToken cancellationToken = default)
        => await _dbContext.Products.AddAsync(entity, cancellationToken);

    public async Task<System.Collections.Generic.IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Products.ToListAsync(cancellationToken);

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.Products.FindAsync(new object[] { id }, cancellationToken);

    public void Remove(Product entity) => _dbContext.Products.Remove(entity);

    public void Update(Product entity) => _dbContext.Products.Update(entity);

    public async Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products.FirstOrDefaultAsync(p => p.Barcode == barcode, cancellationToken);
    }

    public async Task<ProductBalance?> GetBalanceAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProductBalances.FirstOrDefaultAsync(b => b.ProductId == productId, cancellationToken);
    }

    public Task UpdateBalanceAsync(ProductBalance balance, CancellationToken cancellationToken = default)
    {
        _dbContext.ProductBalances.Update(balance);
        return Task.CompletedTask;
    }

    public async Task AddBalanceAsync(ProductBalance balance, CancellationToken cancellationToken = default)
    {
        await _dbContext.ProductBalances.AddAsync(balance, cancellationToken);
    }
}
