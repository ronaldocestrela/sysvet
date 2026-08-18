using Core.Domain.Entities;

namespace Core.Domain.Auditing;

public class AuditLog : Entity
{
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public string EntityName { get; private set; }
    public string Action { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public string PayloadSummary { get; private set; }

#pragma warning disable CS8618
    protected AuditLog() : base(Guid.NewGuid()) { }
#pragma warning restore CS8618

    private AuditLog(Guid id, Guid tenantId, Guid userId, string entityName, string action, DateTimeOffset occurredAt, string payloadSummary)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        EntityName = entityName;
        Action = action;
        OccurredAt = occurredAt;
        PayloadSummary = payloadSummary;
    }

    public static Result<AuditLog> Create(Guid tenantId, Guid userId, string entityName, string action, string payloadSummary, DateTimeOffset occurredAt = default)
    {
        if (string.IsNullOrWhiteSpace(entityName))
            return Result.Failure<AuditLog>(new Error("AuditLog.InvalidEntityName", "O nome da entidade é obrigatório."));
        
        if (string.IsNullOrWhiteSpace(action))
            return Result.Failure<AuditLog>(new Error("AuditLog.InvalidAction", "A ação de auditoria é obrigatória."));

        var log = new AuditLog(Guid.NewGuid(), tenantId, userId, entityName.Trim(), action.Trim(), occurredAt == default ? DateTimeOffset.UtcNow : occurredAt, payloadSummary ?? string.Empty);
        
        return Result.Success(log);
    }
}
