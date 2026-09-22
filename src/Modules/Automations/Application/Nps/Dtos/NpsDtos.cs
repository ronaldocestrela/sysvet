namespace Automations.Application.Nps.Dtos;

/// <summary>
/// Aggregated NPS score for a period.
/// </summary>
public sealed class NpsReportDto
{
    public int TotalResponses { get; init; }
    public int Promoters { get; init; }
    public int Passives { get; init; }
    public int Detractors { get; init; }
    public double NpsScore { get; init; }
    public IReadOnlyList<NpsCommentDto> Comments { get; init; } = Array.Empty<NpsCommentDto>();
}

/// <summary>
/// Verbatim feedback tied to a score.
/// </summary>
public sealed class NpsCommentDto
{
    public int Score { get; init; }
    public string? Comment { get; init; }
    public DateTimeOffset RespondedAt { get; init; }
}

/// <summary>
/// Public survey landing metadata.
/// </summary>
public sealed class NpsPublicPreviewDto
{
    public string TutorName { get; init; } = string.Empty;
    public bool CanRespond { get; init; }
    public string Status { get; init; } = string.Empty;
}

/// <summary>
/// Per-tutor return frequency row for reporting.
/// </summary>
public sealed class ReturnFrequencyReportRowDto
{
    public Guid TutorId { get; init; }
    public string TutorName { get; init; } = string.Empty;
    public int VisitCount { get; init; }
    public DateTimeOffset? LastVisitAt { get; init; }
    public double? AverageDaysBetweenVisits { get; init; }
}
