using Core.Domain.Entitlements;

namespace Core.Application.Entitlements;

/// <summary>
/// Resolves effective commercial modules for a tenant (cached in Platform implementation).
/// </summary>
public interface ITenantEntitlementReader
{
    /// <summary>Returns modules currently enabled for billing and API enforcement.</summary>
    Task<IReadOnlySet<CommercialModule>> GetEnabledModulesAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>True when the tenant may use the given commercial module.</summary>
    Task<bool> IsModuleEnabledAsync(Guid tenantId, CommercialModule module, CancellationToken cancellationToken = default);

    /// <summary>Drops cached entitlements for a tenant after catalog changes.</summary>
    void Invalidate(Guid tenantId);
}
