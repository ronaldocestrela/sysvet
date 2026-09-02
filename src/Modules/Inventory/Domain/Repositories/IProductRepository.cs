using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Domain;
using Inventory.Domain.Entities;

namespace Inventory.Domain.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
    Task<ProductBalance?> GetBalanceAsync(Guid productId, CancellationToken cancellationToken = default);
    Task UpdateBalanceAsync(ProductBalance balance, CancellationToken cancellationToken = default);
    Task AddBalanceAsync(ProductBalance balance, CancellationToken cancellationToken = default);
}
