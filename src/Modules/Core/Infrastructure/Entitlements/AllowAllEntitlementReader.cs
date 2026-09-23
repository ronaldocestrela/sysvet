using Core.Application.Entitlements;
using Core.Domain.Entitlements;

namespace Core.Infrastructure.Entitlements;

/// <summary>
/// Permits all commercial modules when Platform entitlements are not registered (tests and module-only hosts).
/// </summary>
public sealed class AllowAllEntitlementReader : ITenantEntitlementReader
{
    private static readonly HashSet<CommercialModule> AllModules = Enum.GetValues<CommercialModule>().ToHashSet();

    /// <inheritdoc />
    public Task<IReadOnlySet<CommercialModule>> GetEnabledModulesAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlySet<CommercialModule>>(AllModules);

    /// <inheritdoc />
    public Task<bool> IsModuleEnabledAsync(Guid tenantId, CommercialModule module, CancellationToken cancellationToken = default) =>
        Task.FromResult(true);

    /// <inheritdoc />
    public void Invalidate(Guid tenantId)
    {
    }
}
