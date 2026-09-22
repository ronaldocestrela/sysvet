using Platform.Domain.Entities;

namespace Platform.Domain.Repositories;

/// <summary>
/// Persistence port for the global tenant catalog in <c>dbo</c>.
/// </summary>
public interface ITenantRepository
{
    /// <summary>Gets a tenant by primary key.</summary>
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Gets a tenant by normalized slug (includes deleted for internal checks).</summary>
    Task<Tenant?> GetBySlugAsync(string normalizedSlug, CancellationToken cancellationToken = default);

    /// <summary>Lists all registered tenants (multi-tenant workers).</summary>
    Task<IReadOnlyList<Tenant>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>Lists tenants that are active for workers and sign-in.</summary>
    Task<IReadOnlyList<Tenant>> ListActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>True when a non-deleted tenant uses the slug.</summary>
    Task<bool> SlugExistsAsync(string normalizedSlug, CancellationToken cancellationToken = default);

    /// <summary>Adds a tenant row to the catalog.</summary>
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);

    /// <summary>Removes a tenant row (compensation during failed onboarding).</summary>
    Task RemoveAsync(Tenant tenant, CancellationToken cancellationToken = default);
}
