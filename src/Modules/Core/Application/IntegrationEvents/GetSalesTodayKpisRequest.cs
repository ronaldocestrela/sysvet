using Core.Domain;
using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>Aggregates paid POS sales for a UTC half-open interval (Sales module).</summary>
public sealed class GetSalesTodayKpisRequest : IRequest<Result<SalesTodayKpiSnapshot>>
{
    /// <summary>Interval start (UTC, inclusive).</summary>
    public DateTimeOffset PeriodStartUtc { get; }

    /// <summary>Interval end (UTC, exclusive).</summary>
    public DateTimeOffset PeriodEndUtc { get; }

    /// <summary>Creates a sales-day KPI request.</summary>
    public GetSalesTodayKpisRequest(DateTimeOffset periodStartUtc, DateTimeOffset periodEndUtc)
    {
        PeriodStartUtc = periodStartUtc;
        PeriodEndUtc = periodEndUtc;
    }
}
