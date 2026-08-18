namespace Core.Domain.Auditing;

public interface IAuditLogger
{
    Task LogAsync(Guid tenantId, Guid userId, string entityName, string action, string payloadSummary, CancellationToken cancellationToken = default);
}
