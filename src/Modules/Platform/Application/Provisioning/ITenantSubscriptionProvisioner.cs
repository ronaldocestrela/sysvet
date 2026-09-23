using Core.Domain;
using Platform.Domain.Entities;

namespace Platform.Application.Provisioning;

/// <summary>Creates tenant subscription rows after catalog seed (9.3).</summary>
public interface ITenantSubscriptionProvisioner
{
    /// <summary>Starter (or chosen plan) for new onboarding.</summary>
    Task<Result> ProvisionNewTenantAsync(
        Guid tenantId,
        string? planCode,
        int? trialDays,
        TrialEndAction? trialEndAction,
        CancellationToken cancellationToken = default);

    /// <summary>Hospital24h plus all add-ons for existing tenants (dev grandfather).</summary>
    Task<Result> ProvisionGrandfatherAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
