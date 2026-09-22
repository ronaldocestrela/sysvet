namespace Platform.Application.Tenancy;

/// <summary>
/// Resolves a tenant id from a normalized slug (global catalog).
/// </summary>
public interface ITenantSlugLookup
{
    /// <summary>Finds tenant id by slug or returns null when unknown.</summary>
    Task<Guid?> ResolveTenantIdBySlugAsync(string normalizedSlug, CancellationToken cancellationToken = default);
}
