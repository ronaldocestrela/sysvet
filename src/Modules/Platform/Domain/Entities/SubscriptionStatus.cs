namespace Platform.Domain.Entities;

/// <summary>Tenant subscription lifecycle for SaaS billing (9.3).</summary>
public enum SubscriptionStatus
{
    /// <summary>Free trial window; plan modules apply until expiry.</summary>
    Trial = 1,

    /// <summary>Paid or converted active subscription.</summary>
    Active = 2,

    /// <summary>Trial ended with block action; commercial modules disabled.</summary>
    TrialExpired = 3
}
