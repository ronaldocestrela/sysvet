namespace Platform.Domain.Entities;

/// <summary>SaaS payment health for a tenant subscription (9.4); distinct from <see cref="TenantStatus"/>.</summary>
public enum BillingStanding
{
    /// <summary>No billing profile or first cycle not yet invoiced.</summary>
    Unbilled = 0,

    /// <summary>Last invoice paid or current period in good standing.</summary>
    Good = 1,

    /// <summary>Payment overdue; operational block deferred to 9.5.</summary>
    PastDue = 2,

    /// <summary>Subscription billing canceled at gateway.</summary>
    Canceled = 3
}
