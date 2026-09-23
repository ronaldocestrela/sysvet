using Microsoft.EntityFrameworkCore;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Persistence.Repositories;

/// <summary>EF implementation for platform login logs.</summary>
public sealed class PlatformLoginLogRepository : IPlatformLoginLogRepository
{
    private readonly PlatformDbContext _dbContext;

    /// <summary>Creates the repository.</summary>
    public PlatformLoginLogRepository(PlatformDbContext dbContext) => _dbContext = dbContext;

    /// <inheritdoc />
    public Task AddAsync(PlatformLoginLog log, CancellationToken cancellationToken = default) =>
        _dbContext.PlatformLoginLogs.AddAsync(log, cancellationToken).AsTask();

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlatformLoginLog>> ListAsync(Guid? tenantId, int take, CancellationToken cancellationToken = default)
    {
        var (items, _) = await ListPagedAsync(tenantId, 1, take <= 0 ? 100 : Math.Min(take, 500), cancellationToken);
        return items;
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<PlatformLoginLog> Items, int TotalCount)> ListPagedAsync(
        Guid? tenantId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.PlatformLoginLogs.AsNoTracking().AsQueryable();
        if (tenantId is { } id && id != Guid.Empty)
        {
            query = query.Where(l => l.TenantId == id);
        }

        var total = await query.CountAsync(cancellationToken);
        // SQLite cannot ORDER BY DateTimeOffset; sort in memory after the tenant filter.
        var filtered = await query.ToListAsync(cancellationToken);
        var items = filtered
            .OrderByDescending(l => l.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (items, total);
    }
}
