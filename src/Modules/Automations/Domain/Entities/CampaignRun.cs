using Core.Domain;

namespace Automations.Domain.Entities;

/// <summary>
/// Snapshot of a single manual campaign launch (inactive segment).
/// </summary>
public sealed class CampaignRun : Entity
{
    public Guid CampaignId { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public int AudienceCount { get; private set; }
    public int EnqueuedCount { get; private set; }

    private CampaignRun() { }

    /// <summary>
    /// Creates a run row linked to its parent campaign.
    /// </summary>
    public static CampaignRun Create(Guid campaignId, DateTimeOffset startedAt, int audienceCount, int enqueuedCount)
    {
        return new CampaignRun
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            StartedAt = startedAt,
            AudienceCount = audienceCount,
            EnqueuedCount = enqueuedCount
        };
    }
}
