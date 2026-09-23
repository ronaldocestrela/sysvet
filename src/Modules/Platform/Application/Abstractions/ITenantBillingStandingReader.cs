using Platform.Domain.Entities;

namespace Platform.Application.Abstractions;

/// <summary>Reads tenant SaaS billing standing for API enforcement (9.5).</summary>
public interface ITenantBillingStandingReader
{
    /// <summary>Returns billing standing or null when tenant has no subscription row.</summary>
    Task<BillingStanding?> GetStandingAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
