using Automations.Domain.Entities;
using Automations.Domain.Enums;

namespace Automations.Domain.Repositories;

/// <summary>
/// Persistence port for campaign definitions and launch history.
/// </summary>
public interface ICampaignRepository
{
    void Add(Campaign campaign);

    void Update(Campaign campaign);

    Task<Campaign?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Campaign?> GetByIdWithRunsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Campaign>> ListAsync(CancellationToken cancellationToken = default);

    Task<Campaign?> GetActiveBySegmentAsync(CampaignSegmentKind segmentKind, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a manual launch snapshot.
    /// </summary>
    void AddRun(CampaignRun run);
}
