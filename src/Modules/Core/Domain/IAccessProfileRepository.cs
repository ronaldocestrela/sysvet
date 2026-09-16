using Core.Domain.Entities;

namespace Core.Domain;

/// <summary>
/// Persistence port for <see cref="AccessProfile"/> aggregates.
/// </summary>
public interface IAccessProfileRepository : IRepository<AccessProfile>
{
    /// <summary>
    /// Finds a profile by tenant-unique name.
    /// </summary>
    Task<AccessProfile?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the system profile for a base Identity role within the current tenant filter.
    /// </summary>
    Task<AccessProfile?> GetSystemProfileByBaseRoleAsync(string baseRole, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists profiles with optional name filter and paging.
    /// </summary>
    Task<PagedList<AccessProfile>> SearchAsync(int page, int pageSize, string? nameFilter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a profile by id and tenant without the request-scoped query filter (for permission resolution).
    /// </summary>
    Task<AccessProfile?> GetByIdForTenantAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the system profile for a base role within a specific tenant.
    /// </summary>
    Task<AccessProfile?> GetSystemProfileByBaseRoleForTenantAsync(string baseRole, Guid tenantId, CancellationToken cancellationToken = default);
}
