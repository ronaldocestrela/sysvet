using Core.Domain.Entities;

namespace Core.Domain.Auditing;

public class AuditLog : Entity
{
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid EntityId { get; private set; }
    public string EntityName { get; private set; }
    public string Action { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public string PayloadSummary { get; private set; }

#pragma warning disable CS8618
    protected AuditLog() : base(Guid.NewGuid()) { }
#pragma warning restore CS8618

    private AuditLog(Guid id, Guid tenantId, Guid userId, Guid entityId, string entityName, string action, DateTimeOffset occurredAt, string payloadSummary)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        EntityId = entityId;
        EntityName = entityName;
        Action = action;
        OccurredAt = occurredAt;
        PayloadSummary = payloadSummary;
    }

    /// <summary>
    /// Creates an append-only audit entry for a tenant-scoped entity mutation.
    /// </summary>
    public static Result<AuditLog> Create(
        Guid tenantId,
        Guid userId,
        Guid entityId,
        string entityName,
        string action,
        string payloadSummary,
        DateTimeOffset occurredAt = default)
    {
        if (string.IsNullOrWhiteSpace(entityName))
            return Result.Failure<AuditLog>(ErrorCodes.AuditLog.InvalidEntityName);
        
        if (string.IsNullOrWhiteSpace(action))
            return Result.Failure<AuditLog>(ErrorCodes.AuditLog.InvalidAction);

        var log = new AuditLog(
            Guid.NewGuid(),
            tenantId,
            userId,
            entityId,
            entityName.Trim(),
            action.Trim(),
            occurredAt == default ? DateTimeOffset.UtcNow : occurredAt,
            payloadSummary ?? string.Empty);
        
        return Result.Success(log);
    }
}
