using Microsoft.EntityFrameworkCore;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Persistence.Repositories;

/// <summary>EF implementation for daily tenant request counters.</summary>
public sealed class TenantRequestDailyRepository : ITenantRequestDailyRepository
{
    private readonly PlatformDbContext _dbContext;

    /// <summary>Creates the repository.</summary>
    public TenantRequestDailyRepository(PlatformDbContext dbContext) => _dbContext = dbContext;

    /// <inheritdoc />
    public async Task<TenantRequestDaily> GetOrCreateAsync(Guid tenantId, DateTimeOffset asOfUtc, CancellationToken cancellationToken = default)
    {
        var date = DateOnly.FromDateTime(asOfUtc.UtcDateTime);
        var existing = await _dbContext.TenantRequestDailies
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.DateUtc == date, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var created = TenantRequestDaily.Create(tenantId, asOfUtc, 0).Value;
        await _dbContext.TenantRequestDailies.AddAsync(created, cancellationToken);
        return created;
    }

    /// <inheritdoc />
    public Task<TenantRequestDaily?> GetForDateAsync(Guid tenantId, DateOnly dateUtc, CancellationToken cancellationToken = default) =>
        _dbContext.TenantRequestDailies
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.DateUtc == dateUtc, cancellationToken);

    /// <inheritdoc />
    public async Task<int> SumLastDaysAsync(Guid tenantId, int days, CancellationToken cancellationToken = default)
    {
        var span = days <= 0 ? 7 : Math.Min(days, 90);
        var fromDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-span + 1));
        return await _dbContext.TenantRequestDailies
            .AsNoTracking()
            .Where(r => r.TenantId == tenantId && r.DateUtc >= fromDate)
            .SumAsync(r => r.RequestCount, cancellationToken);
    }
}
