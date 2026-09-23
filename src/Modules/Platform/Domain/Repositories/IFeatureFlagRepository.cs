using Core.Domain.Entitlements;
using Platform.Domain.Entities;

namespace Platform.Domain.Repositories;

/// <summary>Feature flag persistence port.</summary>
public interface IFeatureFlagRepository
{
    /// <summary>Lists overrides for a tenant.</summary>
    Task<IReadOnlyList<FeatureFlag>> ListByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Gets a single override.</summary>
    Task<FeatureFlag?> GetAsync(Guid tenantId, CommercialModule module, CancellationToken cancellationToken = default);

    /// <summary>Inserts or updates override.</summary>
    Task UpsertAsync(FeatureFlag flag, CancellationToken cancellationToken = default);
}
