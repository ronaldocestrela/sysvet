using Automations.Application.Abstractions;
using Automations.Domain.Entities;

namespace Automations.Application.Campaigns;

/// <summary>
/// Resolves tutors eligible for inactive re-engagement campaigns.
/// </summary>
public sealed class InactiveCampaignAudienceResolver
{
    private readonly ITutorVisitReadPort _visitReadPort;

    public InactiveCampaignAudienceResolver(ITutorVisitReadPort visitReadPort) => _visitReadPort = visitReadPort;

    /// <summary>
    /// Returns tutors whose last completed visit is older than the campaign threshold (must have a prior visit).
    /// </summary>
    public async Task<IReadOnlyList<InactiveAudienceMember>> ResolveAsync(
        Campaign campaign,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var cutoff = now.AddDays(-campaign.InactiveDays);
        var rows = await _visitReadPort.ListLastCompletedVisitsAsync(cancellationToken);
        return rows
            .Where(r => r.LastVisitAt < cutoff)
            .Select(r => new InactiveAudienceMember(r.TutorId, r.LastVisitAt))
            .ToList();
    }
}

/// <summary>
/// Tutor matched by inactivity segment.
/// </summary>
public sealed record InactiveAudienceMember(Guid TutorId, DateTimeOffset LastVisitAt);
