using Core.Domain;

namespace Platform.Application.Provisioning;

/// <summary>
/// Provisions runtime resources for a newly registered tenant (schema + seed).
/// </summary>
public interface ITenantProvisioner
{
    /// <summary>
    /// Ensures SQL schema (when supported) and seeds default access profiles for the tenant.
    /// </summary>
    Task<Result> ProvisionAsync(Guid tenantId, string schemaName, CancellationToken cancellationToken = default);
}
