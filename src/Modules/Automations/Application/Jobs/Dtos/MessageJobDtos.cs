using Automations.Domain.Enums;

namespace Automations.Application.Jobs.Dtos;

/// <summary>
/// Job list/detail DTO for API and clients.
/// </summary>
public sealed record MessageJobDto(
    Guid Id,
    Guid TenantId,
    string Channel,
    string TemplateCode,
    string Status,
    int AttemptCount,
    int MaxAttempts,
    DateTimeOffset NextAttemptAt,
    string? LastError,
    string? SourceType,
    Guid? SourceId,
    IReadOnlyList<JobAttemptLogDto> AttemptLogs);

/// <summary>
/// Single delivery attempt log line.
/// </summary>
public sealed record JobAttemptLogDto(
    int AttemptNumber,
    string Outcome,
    string? Detail,
    DateTimeOffset StartedAt,
    DateTimeOffset FinishedAt);
