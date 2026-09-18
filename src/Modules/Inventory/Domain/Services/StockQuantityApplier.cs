using Core.Domain;
using Inventory.Domain.Entities;

namespace Inventory.Domain.Services;

/// <summary>
/// Resolves signed quantity delta for a movement type and optional adjustment direction.
/// </summary>
public static class StockQuantityApplier
{
    /// <summary>
    /// Returns the signed delta to apply to on-hand (positive increases stock).
    /// </summary>
    public static Result<decimal> ResolveDelta(
        MovementType type,
        decimal quantity,
        AdjustmentDirection? adjustmentDirection)
    {
        if (quantity <= 0)
        {
            return Result.Failure<decimal>(ErrorCodes.StockMovement.InvalidQuantity);
        }

        return type switch
        {
            MovementType.In => Result.Success(quantity),
            MovementType.Out => Result.Success(-quantity),
            MovementType.Adjustment => ResolveAdjustmentDelta(quantity, adjustmentDirection),
            _ => Result.Failure<decimal>(ErrorCodes.StockMovement.InvalidType)
        };
    }

    /// <summary>
    /// Validates lot requirement and product/lot consistency before applying delta.
    /// </summary>
    public static Result ValidateLotContext(
        Product product,
        ProductLot? lot,
        Guid? productLotId,
        bool hasAnyLots)
    {
        if (product.RequiresLot || hasAnyLots)
        {
            if (productLotId is null || productLotId == Guid.Empty)
            {
                return Result.Failure(ErrorCodes.StockMovement.LotRequired);
            }

            if (lot is null)
            {
                return Result.Failure(ErrorCodes.ProductLot.NotFound);
            }

            if (!lot.IsActive)
            {
                return Result.Failure(ErrorCodes.ProductLot.Inactive);
            }

            if (lot.ProductId != product.Id)
            {
                return Result.Failure(ErrorCodes.StockMovement.LotProductMismatch);
            }
        }

        return Result.Success();
    }

    /// <summary>
    /// Validates a lot-to-lot transfer within the same product.
    /// </summary>
    public static Result ValidateTransferLots(Product product, ProductLot source, ProductLot destination)
    {
        if (source.Id == destination.Id)
        {
            return Result.Failure(ErrorCodes.StockMovement.TransferSameLot);
        }

        if (source.ProductId != product.Id || destination.ProductId != product.Id)
        {
            return Result.Failure(ErrorCodes.StockMovement.TransferProductMismatch);
        }

        if (!source.IsActive || !destination.IsActive)
        {
            return Result.Failure(ErrorCodes.ProductLot.Inactive);
        }

        return Result.Success();
    }

    private static Result<decimal> ResolveAdjustmentDelta(decimal quantity, AdjustmentDirection? direction)
    {
        if (direction is null)
        {
            return Result.Failure<decimal>(ErrorCodes.StockMovement.InvalidAdjustmentDirection);
        }

        return direction == AdjustmentDirection.Increase
            ? Result.Success(quantity)
            : Result.Success(-quantity);
    }
}
