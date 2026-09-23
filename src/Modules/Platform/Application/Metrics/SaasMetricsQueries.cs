using Core.Application.Caching;
using Core.Domain;
using MediatR;

namespace Platform.Application.Metrics;

/// <summary>Loads SaaS KPI snapshot for a civil month (10.2).</summary>
public sealed record GetPlatformSaasMetricsQuery(int Year, int Month) : IRequest<Result<PlatformSaasMetricsDto>>, IPlatformScopedCacheQuery
{
    /// <inheritdoc />
    public string CacheKeySuffix => $"{Year:D4}-{Month:D2}";

    /// <inheritdoc />
    public TimeSpan CacheDuration => CacheDurations.Analytics;
}
