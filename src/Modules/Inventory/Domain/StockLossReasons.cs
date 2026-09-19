namespace Inventory.Domain;

/// <summary>
/// Whitelisted loss motive codes submitted by clients; mapped to ledger reason strings.
/// </summary>
public static class StockLossReasons
{
    /// <summary>Product past expiration date.</summary>
    public const string Expired = "Expired";

    /// <summary>Damaged or unusable stock.</summary>
    public const string Damage = "Damage";

    /// <summary>Internal clinic or operational use.</summary>
    public const string InternalConsumption = "InternalConsumption";

    /// <summary>Donation to third parties.</summary>
    public const string Donation = "Donation";

    private static readonly HashSet<string> ValidCodes = new(StringComparer.Ordinal)
    {
        Expired,
        Damage,
        InternalConsumption,
        Donation
    };

    /// <summary>Returns whether the code is a known loss motive.</summary>
    public static bool IsValid(string? code) =>
        !string.IsNullOrWhiteSpace(code) && ValidCodes.Contains(code.Trim());

    /// <summary>Maps a loss motive to the immutable movement reason code.</summary>
    public static string ToMovementReason(string lossReasonCode) =>
        lossReasonCode.Trim() switch
        {
            Expired => StockMovementReasons.LossExpired,
            Damage => StockMovementReasons.LossDamage,
            InternalConsumption => StockMovementReasons.InternalConsumption,
            Donation => StockMovementReasons.Donation,
            _ => throw new ArgumentOutOfRangeException(nameof(lossReasonCode), lossReasonCode, "Unknown loss reason.")
        };
}
