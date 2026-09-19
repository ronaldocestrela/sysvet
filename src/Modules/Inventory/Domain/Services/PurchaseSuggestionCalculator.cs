namespace Inventory.Domain.Services;

/// <summary>
/// Pure rules for purchase suggestion quantities (no side effects).
/// </summary>
public static class PurchaseSuggestionCalculator
{
    /// <summary>
    /// Returns the stock level used as replenishment target when computing suggestions.
    /// </summary>
    public static decimal EffectiveTargetStock(decimal reorderLevel, decimal targetStock) =>
        targetStock > 0 ? targetStock : reorderLevel;

    /// <summary>
    /// When the product is low stock, returns suggested purchase quantity rounded up to package units; otherwise null.
    /// </summary>
    public static decimal? ComputeSuggestedQuantity(
        decimal onHand,
        decimal reorderLevel,
        decimal targetStock,
        decimal unitsPerPackage)
    {
        if (!StockAlertClassifier.IsLowStock(onHand, reorderLevel))
        {
            return null;
        }

        var effectiveTarget = EffectiveTargetStock(reorderLevel, targetStock);
        var raw = Math.Max(0m, effectiveTarget - onHand);
        if (raw <= 0m)
        {
            return null;
        }

        return CeilToPackage(raw, unitsPerPackage);
    }

    /// <summary>
    /// Rounds quantity up to the nearest multiple of units per package (minimum one package unit).
    /// </summary>
    public static decimal CeilToPackage(decimal rawQuantity, decimal unitsPerPackage)
    {
        var pack = unitsPerPackage <= 0m ? 1m : unitsPerPackage;
        var packages = Math.Ceiling(rawQuantity / pack);
        return packages * pack;
    }
}
