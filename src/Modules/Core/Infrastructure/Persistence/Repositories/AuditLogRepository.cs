using Core.Domain;
using Core.Domain.Auditing;
using Microsoft.EntityFrameworkCore;

namespace Core.Infrastructure.Persistence.Repositories;

/// <summary>
/// Tenant-scoped read access to append-only audit logs.
/// </summary>
public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly CoreDbContext _dbContext;

    public AuditLogRepository(CoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<PagedList<AuditLog>> SearchAsync(
        int page,
        int pageSize,
        string? entityName,
        Guid? entityId,
        string? action,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            query = query.Where(a => a.EntityName == entityName.Trim());
        }

        if (entityId.HasValue)
        {
            query = query.Where(a => a.EntityId == entityId.Value);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(a => a.Action == action.Trim());
        }

        if (from.HasValue)
        {
            query = query.Where(a => a.OccurredAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(a => a.OccurredAt <= to.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        // SQLite cannot ORDER BY DateTimeOffset; sort in memory after filters (admin audit volumes are bounded per page).
        var filtered = await query.ToListAsync(cancellationToken);
        var items = filtered
            .OrderByDescending(a => a.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedList<AuditLog>(items, total);
    }
}
