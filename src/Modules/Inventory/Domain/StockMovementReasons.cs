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

    /// <summary>Loss due to expiration.</summary>
    public const string LossExpired = "LossExpired";

    /// <summary>Loss due to damage.</summary>
    public const string LossDamage = "LossDamage";

    /// <summary>Internal consumption write-off.</summary>
    public const string InternalConsumption = "InternalConsumption";

    /// <summary>Donation write-off.</summary>
    public const string Donation = "Donation";

    /// <summary>Return to supplier.</summary>
    public const string SupplierReturn = "SupplierReturn";

    /// <summary>Package opened into fractional lot.</summary>
    public const string Fractionation = "Fractionation";

    /// <summary>Physical inventory count adjustment after approval.</summary>
    public const string InventoryCount = "InventoryCount";
}
