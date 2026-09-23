using TutorPortal.Domain.Entities;

namespace TutorPortal.Domain.Repositories;

/// <summary>
/// Persistence port for <see cref="TutorPortalAccount"/> linkages.
/// </summary>
public interface ITutorPortalAccountRepository
{
    /// <summary>
    /// Adds a new account linkage pending unit-of-work commit.
    /// </summary>
    void Add(TutorPortalAccount account);

    /// <summary>
    /// Finds the linkage for an Identity user within the current tenant scope.
    /// </summary>
    Task<TutorPortalAccount?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the linkage for a CRM tutor within the current tenant scope.
    /// </summary>
    Task<TutorPortalAccount?> GetByTutorIdAsync(Guid tutorId, CancellationToken cancellationToken = default);

    /// <summary>Removes a portal linkage pending unit-of-work commit.</summary>
    void Remove(TutorPortalAccount account);
}
