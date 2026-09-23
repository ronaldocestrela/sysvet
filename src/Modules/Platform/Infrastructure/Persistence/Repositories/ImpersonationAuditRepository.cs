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
    public Task<IReadOnlyList<ImpersonationAuditEntry>> ListAsync(int take, CancellationToken cancellationToken = default) =>
        _context.ImpersonationAuditEntries
            .AsNoTracking()
            .OrderByDescending(e => e.OccurredAt)
            .Take(take)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<ImpersonationAuditEntry>)t.Result, cancellationToken);
}
