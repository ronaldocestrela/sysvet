namespace Platform.Infrastructure.Entitlements;

/// <summary>Cache key helpers for tenant entitlements (ADR-057).</summary>
internal static class EntitlementCacheKeys
{
    /// <summary>Builds the distributed cache key for a tenant's effective modules.</summary>
    public static string ForTenant(Guid tenantId) => $"sysvet:entitlement:{tenantId:N}";
}
