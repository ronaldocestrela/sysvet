using Core.Domain.Auditing;

namespace Core.Application.AuditLogs.Queries;

internal static class AuditLogMappings
{
    public static AuditLogDto ToDto(AuditLog log) =>
        new(
            log.Id,
            log.UserId,
            log.EntityId,
            log.EntityName,
            log.Action,
            log.OccurredAt,
            log.PayloadSummary);
}
