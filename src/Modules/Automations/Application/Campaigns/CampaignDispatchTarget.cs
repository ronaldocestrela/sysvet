namespace Automations.Application.Campaigns;

/// <summary>
/// Outbound marketing/NPS target ready to be enqueued as message jobs.
/// </summary>
public sealed record CampaignDispatchTarget(
    Guid TutorId,
    string TemplateCode,
    string IdempotencyKeyBase,
    IReadOnlyDictionary<string, string> Tokens,
    string? SourceType = null,
    Guid? SourceId = null);
