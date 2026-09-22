using Automations.Domain.Enums;

namespace Automations.Application.Campaigns.Dtos;

/// <summary>
/// Campaign list/detail projection.
/// </summary>
public sealed class CampaignDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public CampaignSegmentKind SegmentKind { get; init; }
    public CampaignStatus Status { get; init; }
    public string TemplateCode { get; init; } = string.Empty;
    public int InactiveDays { get; init; }
    public int CooldownDays { get; init; }
}

/// <summary>
/// Manual launch metrics.
/// </summary>
public sealed class CampaignRunDto
{
    public Guid Id { get; init; }
    public Guid CampaignId { get; init; }
    public DateTimeOffset StartedAt { get; init; }
    public int AudienceCount { get; init; }
    public int EnqueuedCount { get; init; }
}
