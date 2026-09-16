using Core.Domain.Entities;

namespace Core.Domain;

/// <summary>
/// Persistence port for <see cref="UserPreference"/> entities.
/// </summary>
public interface IUserPreferenceRepository : IRepository<UserPreference>
{
    /// <summary>
    /// Loads preferences for the given Identity user id.
    /// </summary>
    Task<UserPreference?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
