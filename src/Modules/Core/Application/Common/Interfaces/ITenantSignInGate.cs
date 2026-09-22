using Core.Domain;

namespace Core.Application.Common.Interfaces;

/// <summary>
/// Validates whether a tenant account may authenticate or call operational APIs (Platform 9.2).
/// </summary>
public interface ITenantSignInGate
{
    /// <summary>
    /// Ensures the tenant exists in the catalog and is <c>Active</c>.
    /// </summary>
    Task<Result> EnsureActiveAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
