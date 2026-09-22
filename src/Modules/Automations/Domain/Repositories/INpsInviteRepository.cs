using Automations.Domain.Entities;

namespace Automations.Domain.Repositories;

/// <summary>
/// Persistence port for NPS survey invitations and responses.
/// </summary>
public interface INpsInviteRepository
{
    void Add(NpsInvite invite);

    void Update(NpsInvite invite);

    Task<NpsInvite?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsForSourceAsync(string sourceType, Guid sourceId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NpsInvite>> ListRespondedBetweenAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);
}
