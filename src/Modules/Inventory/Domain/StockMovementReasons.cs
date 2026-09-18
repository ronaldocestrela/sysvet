namespace Inventory.Domain;

/// <summary>
/// Well-known stock movement reason codes for audit and kardex.
/// </summary>
public static class StockMovementReasons
{
    /// <summary>Initial quantity when registering a product lot.</summary>
    public const string OpeningBalance = "OpeningBalance";

    /// <summary>Manual or purchase entry.</summary>
    public const string Purchase = "Purchase";

    /// <summary>Point-of-sale or clinical consumption.</summary>
    public const string Sale = "Sale";

    /// <summary>Lot-to-lot transfer within the same product.</summary>
    public const string Transfer = "Transfer";

    /// <summary>Inventory correction.</summary>
    public const string Adjustment = "Adjustment";
}
