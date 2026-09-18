namespace Inventory.Domain;

/// <summary>
/// Well-known stock movement reason codes for audit and kardex.
/// </summary>
public static class StockMovementReasons
{
    /// <summary>Initial quantity when registering a product lot.</summary>
    public const string OpeningBalance = "OpeningBalance";
}
