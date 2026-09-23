using Platform.Domain.Entities;

namespace Platform.Domain.Repositories;

/// <summary>
/// Persistence port for tenant branches in the global catalog.
/// </summary>
public interface IBranchRepository
{
    /// <summary>Gets a branch by id when owned by the tenant.</summary>
    Task<Branch?> GetByIdAsync(Guid tenantId, Guid branchId, CancellationToken cancellationToken = default);

    /// <summary>Lists non-deleted branches for a tenant.</summary>
    Task<IReadOnlyList<Branch>> ListByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Gets the headquarters branch when present.</summary>
    Task<Branch?> GetHeadquartersAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>True when an active branch already uses the CNPJ for the tenant.</summary>
    Task<bool> CnpjExistsAsync(Guid tenantId, string cnpjDigits, Guid? excludeBranchId = null, CancellationToken cancellationToken = default);

    /// <summary>True when the tenant already has a headquarters branch.</summary>
    Task<bool> HeadquartersExistsAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Adds a branch row.</summary>
    Task AddAsync(Branch branch, CancellationToken cancellationToken = default);

    /// <summary>Removes a branch row (compensation during failed onboarding).</summary>
    Task RemoveAsync(Branch branch, CancellationToken cancellationToken = default);
}
