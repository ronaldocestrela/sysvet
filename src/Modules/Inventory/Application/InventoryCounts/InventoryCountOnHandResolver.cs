using Core.Domain;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using InvErrors = Inventory.Domain.ErrorCodes;

namespace Inventory.Application.InventoryCounts;

/// <summary>
/// Resolves current on-hand quantity for count submit and approve.
/// </summary>
public sealed class InventoryCountOnHandResolver
{
    private readonly IProductRepository _productRepository;
    private readonly IProductLotRepository _lotRepository;

    public InventoryCountOnHandResolver(IProductRepository productRepository, IProductLotRepository lotRepository)
    {
        _productRepository = productRepository;
        _lotRepository = lotRepository;
    }

    /// <summary>Returns lot quantity or product balance total.</summary>
    public async Task<Result<decimal>> GetOnHandAsync(
        Guid productId,
        Guid? productLotId,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        if (product is null || !product.IsActive)
        {
            return Result.Failure<decimal>(InvErrors.Product.NotFound);
        }

        var lots = await _lotRepository.ListByProductIdAsync(productId, cancellationToken);
        var useLotPath = product.RequiresLot || lots.Count > 0;

        if (useLotPath)
        {
            if (productLotId is null)
            {
                return Result.Failure<decimal>(InvErrors.InventoryCount.LotRequired);
            }

            var lot = lots.FirstOrDefault(l => l.Id == productLotId);
            if (lot is null || !lot.IsActive)
            {
                return Result.Failure<decimal>(InvErrors.ProductLot.NotFound);
            }

            return Result.Success(lot.Quantity);
        }

        var balance = await _productRepository.GetBalanceAsync(productId, cancellationToken);
        return Result.Success(balance?.TotalQuantity ?? 0m);
    }
}
