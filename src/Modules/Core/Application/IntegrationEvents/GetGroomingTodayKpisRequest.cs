using Core.Domain;
using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>Counts grooming appointments scheduled on the business day (Petshop module).</summary>
public sealed class GetGroomingTodayKpisRequest : IRequest<Result<GroomingTodayKpiSnapshot>>
{
    /// <summary>Interval start (UTC, inclusive).</summary>
    public DateTimeOffset PeriodStartUtc { get; }

    /// <summary>Interval end (UTC, exclusive).</summary>
    public DateTimeOffset PeriodEndUtc { get; }

    /// <summary>Creates a grooming-day KPI request.</summary>
    public GetGroomingTodayKpisRequest(DateTimeOffset periodStartUtc, DateTimeOffset periodEndUtc)
    {
        PeriodStartUtc = periodStartUtc;
        PeriodEndUtc = periodEndUtc;
    }
}
