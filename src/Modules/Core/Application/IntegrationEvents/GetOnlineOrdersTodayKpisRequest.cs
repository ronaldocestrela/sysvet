using Core.Domain;
using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>Aggregates confirmed online orders for the business day (Commerce module).</summary>
public sealed class GetOnlineOrdersTodayKpisRequest : IRequest<Result<OnlineOrdersTodayKpiSnapshot>>
{
    /// <summary>Interval start (UTC, inclusive).</summary>
    public DateTimeOffset PeriodStartUtc { get; }

    /// <summary>Interval end (UTC, exclusive).</summary>
    public DateTimeOffset PeriodEndUtc { get; }

    /// <summary>Creates an online orders KPI request.</summary>
    public GetOnlineOrdersTodayKpisRequest(DateTimeOffset periodStartUtc, DateTimeOffset periodEndUtc)
    {
        PeriodStartUtc = periodStartUtc;
        PeriodEndUtc = periodEndUtc;
    }
}
