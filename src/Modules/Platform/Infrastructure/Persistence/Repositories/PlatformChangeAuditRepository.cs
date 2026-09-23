using Microsoft.EntityFrameworkCore;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Persistence.Repositories;

/// <summary>EF implementation for platform change audits.</summary>
public sealed class PlatformChangeAuditRepository : IPlatformChangeAuditRepository
{
    private readonly PlatformDbContext _dbContext;

    /// <summary>Creates the repository.</summary>
    public PlatformChangeAuditRepository(PlatformDbContext dbContext) => _dbContext = dbContext;

    /// <inheritdoc />
    public Task AddAsync(PlatformChangeAuditEntry entry, CancellationToken cancellationToken = default) =>
        _dbContext.PlatformChangeAuditEntries.AddAsync(entry, cancellationToken).AsTask();

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlatformChangeAuditEntry>> ListAsync(Guid? tenantId, int take, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.PlatformChangeAuditEntries.AsNoTracking().AsQueryable();
        if (tenantId is { } id && id != Guid.Empty)
        {
            query = query.Where(e => e.TenantId == id);
        }

        var limit = take <= 0 ? 100 : Math.Min(take, 500);
        var filtered = await query.ToListAsync(cancellationToken);
        return filtered
            .OrderByDescending(e => e.OccurredAt)
            .Take(limit)
            .ToList();
    }
}
