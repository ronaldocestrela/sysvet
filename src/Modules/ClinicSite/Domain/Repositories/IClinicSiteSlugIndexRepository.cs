using ClinicSite.Domain.Entities;

namespace ClinicSite.Domain.Repositories;

/// <summary>
/// Global slug registry (cross-tenant lookup for anonymous visitors).
/// </summary>
public interface IClinicSiteSlugIndexRepository
{
    /// <summary>
    /// Finds a slug entry by normalized slug text.
    /// </summary>
    Task<ClinicSiteSlugIndex?> GetBySlugAsync(string normalizedSlug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the slug entry owned by a tenant, if any.
    /// </summary>
    Task<ClinicSiteSlugIndex?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a new slug reservation.
    /// </summary>
    Task AddAsync(ClinicSiteSlugIndex index, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing slug row.
    /// </summary>
    Task UpdateAsync(ClinicSiteSlugIndex index, CancellationToken cancellationToken = default);
}
