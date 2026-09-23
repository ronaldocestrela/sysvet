using Platform.Domain.Entities;

namespace Platform.Domain.Repositories;

/// <summary>Daily tenant request counters (9.7).</summary>
public interface ITenantRequestDailyRepository
{
    /// <summary>Gets or creates the counter row for the UTC date.</summary>
    Task<TenantRequestDaily> GetOrCreateAsync(Guid tenantId, DateTimeOffset asOfUtc, CancellationToken cancellationToken = default);

    /// <summary>Gets today's counter when present.</summary>
    Task<TenantRequestDaily?> GetForDateAsync(Guid tenantId, DateOnly dateUtc, CancellationToken cancellationToken = default);

    /// <summary>Sums request counts for the last N UTC days.</summary>
    Task<int> SumLastDaysAsync(Guid tenantId, int days, CancellationToken cancellationToken = default);
}
