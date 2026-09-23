using Platform.Domain.Entities;

namespace Platform.Domain.Repositories;

/// <summary>Tenant subscription persistence port.</summary>
public interface ITenantSubscriptionRepository
{
    /// <summary>Gets subscription with plan modules and active add-ons.</summary>
    Task<TenantSubscription?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Lists trials past end date.</summary>
    Task<IReadOnlyList<TenantSubscription>> ListDueTrialsAsync(DateTimeOffset asOfUtc, CancellationToken cancellationToken = default);

    /// <summary>Adds subscription row.</summary>
    Task AddAsync(TenantSubscription subscription, CancellationToken cancellationToken = default);
}
