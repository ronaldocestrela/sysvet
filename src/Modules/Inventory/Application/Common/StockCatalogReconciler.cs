using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;

namespace Inventory.Application.Common;

/// <summary>
/// Reconciles product balance and average cost after lot quantity changes.
/// </summary>
public sealed class StockCatalogReconciler
{
    private readonly IProductRepository _productRepository;
    private readonly IProductLotRepository _lotRepository;

    /// <summary>Creates the reconciler.</summary>
    public StockCatalogReconciler(IProductRepository productRepository, IProductLotRepository lotRepository)
    {
        _productRepository = productRepository;
        _lotRepository = lotRepository;
    }

    /// <summary>Reloads lots and applies balance/cost projection for the product.</summary>
    public async Task ReconcileAsync(Product product, CancellationToken cancellationToken)
    {
        var balance = await _productRepository.GetBalanceAsync(product.Id, cancellationToken);
        if (balance is null)
        {
            balance = new ProductBalance(product.Id, 0m);
            await _productRepository.AddBalanceAsync(balance, cancellationToken);
        }

        var lots = await _lotRepository.ListByProductIdAsync(product.Id, cancellationToken);
        ProductCatalogProjection.ApplyLotSnapshot(product, balance, lots);
        _productRepository.Update(product);
        await _productRepository.UpdateBalanceAsync(balance, cancellationToken);
    }
}
