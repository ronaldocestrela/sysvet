namespace Platform.Application.Auditing;

/// <summary>Semantic action names for Super Admin change audit (9.7).</summary>
public static class PlatformChangeActions
{
    /// <summary>Tenant subscription plan changed.</summary>
    public const string PlanChanged = "PlanChanged";

    /// <summary>Add-on activated on subscription.</summary>
    public const string AddOnActivated = "AddOnActivated";

    /// <summary>Add-on deactivated on subscription.</summary>
    public const string AddOnDeactivated = "AddOnDeactivated";

    /// <summary>Feature flag upserted for tenant.</summary>
    public const string FeatureFlagSet = "FeatureFlagSet";

    /// <summary>Global coupon created.</summary>
    public const string CouponCreated = "CouponCreated";

    /// <summary>Coupon redeemed for tenant billing.</summary>
    public const string CouponRedeemed = "CouponRedeemed";

    /// <summary>Partner API key issued.</summary>
    public const string ApiKeyCreated = "ApiKeyCreated";

    /// <summary>Partner API key revoked.</summary>
    public const string ApiKeyRevoked = "ApiKeyRevoked";
}
