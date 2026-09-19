using Core.Domain;
using Inventory.Domain.Entities;

namespace Inventory.Domain.Services;

/// <summary>
/// Plans opening sealed packages into a fractional lot on the same SKU.
/// </summary>
public static class PackageFractionationService
{
    /// <summary>Suffix appended to the sealed lot number for the open-stock bucket.</summary>
    public const string FractionalLotSuffix = "-F";

    /// <summary>Outcome of a valid fractionation plan (no persistence).</summary>
    public sealed record FractionationPlan(
        Guid SourceLotId,
        string TargetLotNumber,
        decimal QuantityToTransfer,
        DateTimeOffset? ExpirationDate,
        decimal UnitCost);

    /// <summary>
    /// Validates inputs and computes how much stock moves from sealed to fractional lot.
    /// </summary>
    public static Result<FractionationPlan> Plan(Product product, ProductLot sealedLot, decimal packagesToOpen)
    {
        if (product.UnitsPerPackage <= 1m)
        {
            return Result.Failure<FractionationPlan>(ErrorCodes.Product.InvalidUnitsPerPackage);
        }

        if (packagesToOpen <= 0)
        {
            return Result.Failure<FractionationPlan>(ErrorCodes.StockMovement.InvalidQuantity);
        }

        if (!sealedLot.IsActive)
        {
            return Result.Failure<FractionationPlan>(ErrorCodes.ProductLot.Inactive);
        }

        if (sealedLot.IsFractional)
        {
            return Result.Failure<FractionationPlan>(ErrorCodes.StockMovement.CannotFractionateOpenLot);
        }

        if (sealedLot.ProductId != product.Id)
        {
            return Result.Failure<FractionationPlan>(ErrorCodes.StockMovement.LotProductMismatch);
        }

        var quantity = packagesToOpen * product.UnitsPerPackage;
        if (sealedLot.Quantity < quantity)
        {
            return Result.Failure<FractionationPlan>(ErrorCodes.ProductLot.InsufficientQuantity);
        }

        var targetLotNumber = sealedLot.LotNumber + FractionalLotSuffix;
        return Result.Success(new FractionationPlan(
            sealedLot.Id,
            targetLotNumber,
            quantity,
            sealedLot.ExpirationDate,
            sealedLot.UnitCost));
    }
}
