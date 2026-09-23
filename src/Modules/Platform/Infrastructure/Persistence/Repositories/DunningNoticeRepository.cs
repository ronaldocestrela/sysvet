using Microsoft.EntityFrameworkCore;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;
using Platform.Domain.Services;

namespace Platform.Infrastructure.Persistence.Repositories;

/// <inheritdoc />
public sealed class DunningNoticeRepository : IDunningNoticeRepository
{
    private readonly PlatformDbContext _context;

    /// <summary>Creates the repository.</summary>
    public DunningNoticeRepository(PlatformDbContext context) => _context = context;

    /// <inheritdoc />
    public async Task<IReadOnlySet<string>> ListSentKeysAsync(
        Guid tenantId,
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        var notices = await _context.DunningNotices
            .AsNoTracking()
            .Where(n => n.TenantId == tenantId && n.InvoiceId == invoiceId && n.SentAt != null)
            .ToListAsync(cancellationToken);

        var keys = new HashSet<string>();
        foreach (var notice in notices)
        {
            keys.Add(DunningScheduleCalculator.BuildKey(notice.StepDay, notice.Channel));
        }

        return keys;
    }

    /// <inheritdoc />
    public async Task AddAsync(DunningNotice notice, CancellationToken cancellationToken = default) =>
        await _context.DunningNotices.AddAsync(notice, cancellationToken);
}
