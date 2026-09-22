using Platform.Domain.Entities;

namespace Platform.Domain.Repositories;

/// <summary>
/// Persistence port for the global tenant catalog in <c>dbo</c>.
/// </summary>
public interface ITenantRepository
{
    /// <summary>Gets a tenant by primary key.</summary>
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Gets a tenant by normalized slug.</summary>
    Task<Tenant?> GetBySlugAsync(string normalizedSlug, CancellationToken cancellationToken = default);

    /// <summary>Lists all registered tenants (multi-tenant workers).</summary>
    Task<IReadOnlyList<Tenant>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>Adds a tenant row to the catalog.</summary>
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);
}
