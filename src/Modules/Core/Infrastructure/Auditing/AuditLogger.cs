using Core.Domain.Auditing;
using Core.Infrastructure.Persistence;

namespace Core.Infrastructure.Auditing;

public class AuditLogger : IAuditLogger
{
    private readonly CoreDbContext _dbContext;

    public AuditLogger(CoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LogAsync(Guid tenantId, Guid userId, string entityName, string action, string payloadSummary, CancellationToken cancellationToken = default)
    {
        var auditLogResult = AuditLog.Create(tenantId, userId, entityName, action, payloadSummary);
        if (auditLogResult.IsSuccess)
        {
            _dbContext.AuditLogs.Add(auditLogResult.Value);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
