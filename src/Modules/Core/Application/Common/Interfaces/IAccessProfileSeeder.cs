namespace Core.Application.Common.Interfaces;

/// <summary>
/// Ensures system access profiles exist for a tenant (idempotent).
/// </summary>
public interface IAccessProfileSeeder
{
    /// <summary>
    /// Creates the four system profiles for the tenant when missing.
    /// </summary>
    Task EnsureTenantProfilesAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
