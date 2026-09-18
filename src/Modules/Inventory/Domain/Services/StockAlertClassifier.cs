namespace Inventory.Domain.Services;

/// <summary>
/// Pure rules for low-stock and lot expiry alerts (no side effects).
/// </summary>
public static class StockAlertClassifier
{
    /// <summary>
    /// True when on-hand is at or below the configured reorder level.
    /// </summary>
    public static bool IsLowStock(decimal totalQuantity, decimal reorderLevel) =>
        reorderLevel > 0 && totalQuantity <= reorderLevel;

    /// <summary>
    /// Classifies an active lot expiration date against UTC now and horizon (inclusive).
    /// </summary>
    public static StockAlertKind ClassifyLotExpiry(DateTimeOffset? expirationDate, DateTimeOffset utcNow, int horizonDays)
    {
        if (expirationDate is null)
        {
            return StockAlertKind.None;
        }

        if (expirationDate.Value < utcNow)
        {
            return StockAlertKind.Expired;
        }

        var horizonEnd = utcNow.AddDays(horizonDays);
        if (expirationDate.Value <= horizonEnd)
        {
            return StockAlertKind.ExpiringSoon;
        }

        return StockAlertKind.None;
    }
}
