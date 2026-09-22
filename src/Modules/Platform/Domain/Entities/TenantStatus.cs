namespace Platform.Domain.Entities;

/// <summary>
/// Lifecycle state of a SaaS tenant account (roadmap 9.2).
/// </summary>
public enum TenantStatus
{
    /// <summary>Tenant may sign in and use operational APIs.</summary>
    Active = 0,

    /// <summary>Access blocked until reactivated (billing/support).</summary>
    Suspended = 1,

    /// <summary>Subscription ended; data retained.</summary>
    Cancelled = 2,

    /// <summary>Soft-deleted catalog row; slug may be reused.</summary>
    Deleted = 3
}
