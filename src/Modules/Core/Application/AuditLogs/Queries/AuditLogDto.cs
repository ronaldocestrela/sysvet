namespace Core.Application.AuditLogs.Queries;

/// <summary>
/// Read model for a single append-only audit entry.
/// </summary>
public sealed record AuditLogDto(
    Guid Id,
    Guid UserId,
    Guid EntityId,
    string EntityName,
    string Action,
    DateTimeOffset OccurredAt,
    string PayloadSummary);
