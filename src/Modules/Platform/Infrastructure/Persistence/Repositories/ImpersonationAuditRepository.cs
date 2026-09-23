using Microsoft.EntityFrameworkCore;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Persistence.Repositories;

/// <inheritdoc />
public sealed class ImpersonationAuditRepository : IImpersonationAuditRepository
{
    private readonly PlatformDbContext _context;

    /// <summary>Creates the repository.</summary>
    public ImpersonationAuditRepository(PlatformDbContext context) => _context = context;

    /// <inheritdoc />
    public async Task AddAsync(ImpersonationAuditEntry entry, CancellationToken cancellationToken = default) =>
        await _context.ImpersonationAuditEntries.AddAsync(entry, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<ImpersonationAuditEntry>> ListAsync(int take, CancellationToken cancellationToken = default)
    {
        var (items, _) = await ListPagedAsync(1, take, cancellationToken);
        return items;
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<ImpersonationAuditEntry> Items, int TotalCount)> ListPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ImpersonationAuditEntries.AsNoTracking();
        var total = await query.CountAsync(cancellationToken);
        // SQLite cannot ORDER BY DateTimeOffset; sort in memory after the filter.
        var filtered = await query.ToListAsync(cancellationToken);
        var items = filtered
            .OrderByDescending(e => e.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (items, total);
    }
}
