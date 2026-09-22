using ClinicSite.Domain.Entities;

namespace ClinicSite.Domain.Repositories;

/// <summary>
/// Persistence port for the tenant clinic site profile aggregate.
/// </summary>
public interface IClinicSiteProfileRepository
{
    /// <summary>
    /// Loads the singleton profile with children, or null when never created.
    /// </summary>
    Task<ClinicSiteProfile?> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists the profile aggregate (insert or update).
    /// </summary>
    Task AddAsync(ClinicSiteProfile profile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the aggregate as modified in the current unit of work.
    /// </summary>
    Task UpdateAsync(ClinicSiteProfile profile, CancellationToken cancellationToken = default);
}
