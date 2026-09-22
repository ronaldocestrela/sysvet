namespace ClinicSite.Domain.Repositories;

/// <summary>
/// Unit of work for ClinicSite module persistence.
/// </summary>
public interface IClinicSiteUnitOfWork
{
    /// <summary>
    /// Persists pending changes.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
