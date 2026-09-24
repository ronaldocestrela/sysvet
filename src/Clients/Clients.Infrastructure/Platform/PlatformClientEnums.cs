namespace Clients.Infrastructure.Platform;

/// <summary>Mirror of Platform tenant lifecycle for JSON deserialization.</summary>
public enum PlatformTenantStatus
{
    Active = 0,
    Suspended = 1,
    Cancelled = 2,
    Deleted = 3
}

/// <summary>Mirror of platform release ring (10.7).</summary>
public enum PlatformReleaseRing
{
    Canary = 0,
    Beta = 1,
    GeneralAvailability = 2
}

/// <summary>Mirror of public status incident impact.</summary>
public enum PlatformStatusIncidentImpact
{
    None = 0,
    Minor = 1,
    Major = 2,
    Critical = 3
}

/// <summary>Mirror of platform feature flag override state.</summary>
public enum PlatformFeatureFlagState
{
    Enabled = 1,
    Disabled = 2
}

/// <summary>Mirror of coupon discount kind.</summary>
public enum PlatformCouponDiscountType
{
    Percent = 0,
    FixedAmount = 1
}

/// <summary>Mirror of SaaS billing payment rail.</summary>
public enum PlatformBillingPaymentMethodKind
{
    CreditCard = 1,
    Pix = 2,
    Boleto = 3
}

/// <summary>Mirror of billing invoice status.</summary>
public enum PlatformBillingInvoiceStatus
{
    Open = 0,
    Paid = 1,
    Failed = 2,
    Cancelled = 3
}
