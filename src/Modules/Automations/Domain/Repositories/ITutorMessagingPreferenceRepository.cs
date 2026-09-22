using Automations.Domain.Entities;

namespace Automations.Domain.Repositories;

/// <summary>
/// Persistence port for tutor messaging opt-out preferences.
/// </summary>
public interface ITutorMessagingPreferenceRepository
{
    Task<TutorMessagingPreference?> GetByTutorIdAsync(Guid tutorId, CancellationToken cancellationToken = default);

    void Add(TutorMessagingPreference preference);

    void Update(TutorMessagingPreference preference);
}
