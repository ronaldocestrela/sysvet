using Automations.Domain.Enums;
using Core.Domain;

namespace Automations.Domain.Entities;

/// <summary>
/// Marketing or NPS campaign definition scoped to the current tenant schema.
/// </summary>
public sealed class Campaign : AggregateRoot
{
    public const int DefaultInactiveDays = 90;
    public const int DefaultCooldownDays = 90;

    public string Name { get; private set; } = string.Empty;
    public CampaignSegmentKind SegmentKind { get; private set; }
    public CampaignStatus Status { get; private set; }
    public string TemplateCode { get; private set; } = string.Empty;
    public int InactiveDays { get; private set; }
    public int CooldownDays { get; private set; }

    private readonly List<CampaignRun> _runs = new();

    /// <summary>Historical launch metrics for manual inactive campaigns.</summary>
    public IReadOnlyCollection<CampaignRun> Runs => _runs.AsReadOnly();

    private Campaign() { }

    /// <summary>
    /// Creates a new campaign in draft with segment-specific default template codes.
    /// </summary>
    public static Result<Campaign> Create(
        string name,
        CampaignSegmentKind segmentKind,
        string? templateCode = null,
        int inactiveDays = DefaultInactiveDays,
        int cooldownDays = DefaultCooldownDays,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Campaign>(ErrorCodes.Campaign.InvalidName);
        }

        if (inactiveDays <= 0 || cooldownDays <= 0)
        {
            return Result.Failure<Campaign>(ErrorCodes.Campaign.InvalidThresholds);
        }

        var code = string.IsNullOrWhiteSpace(templateCode)
            ? DefaultTemplateFor(segmentKind)
            : templateCode.Trim();

        return Result.Success(new Campaign
        {
            Id = id ?? Guid.NewGuid(),
            Name = name.Trim(),
            SegmentKind = segmentKind,
            Status = CampaignStatus.Draft,
            TemplateCode = code,
            InactiveDays = inactiveDays,
            CooldownDays = cooldownDays
        });
    }

    /// <summary>
    /// Updates editable fields while the campaign is not removed from the catalog.
    /// </summary>
    public Result Update(string name, string templateCode, int inactiveDays, int cooldownDays, CampaignStatus status)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(ErrorCodes.Campaign.InvalidName);
        }

        if (string.IsNullOrWhiteSpace(templateCode))
        {
            return Result.Failure(ErrorCodes.Campaign.InvalidTemplate);
        }

        if (inactiveDays <= 0 || cooldownDays <= 0)
        {
            return Result.Failure(ErrorCodes.Campaign.InvalidThresholds);
        }

        Name = name.Trim();
        TemplateCode = templateCode.Trim();
        InactiveDays = inactiveDays;
        CooldownDays = cooldownDays;
        Status = status;
        return Result.Success();
    }

    /// <summary>
    /// Records a manual launch attempt for inactive campaigns.
    /// </summary>
    public CampaignRun RecordRun(int audienceCount, int enqueuedCount, DateTimeOffset startedAt)
    {
        var run = CampaignRun.Create(Id, startedAt, audienceCount, enqueuedCount);
        _runs.Add(run);
        return run;
    }

    /// <summary>
    /// Whether post-appointment scanning should enqueue NPS for this campaign.
    /// </summary>
    public bool IsActivePostAppointmentNps() =>
        SegmentKind == CampaignSegmentKind.PostAppointment && Status == CampaignStatus.Active;

    private static string DefaultTemplateFor(CampaignSegmentKind kind) =>
        kind switch
        {
            CampaignSegmentKind.Inactive90Days => "campaign.inactive",
            CampaignSegmentKind.PostAppointment => "nps.request",
            _ => "campaign.inactive"
        };
}
