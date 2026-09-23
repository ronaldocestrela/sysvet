using Core.Domain;
using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>Aggregates paid POS sales for ABC and sales productivity (Sales module).</summary>
public sealed class GetSalesReportAggregatesRequest : IRequest<Result<SalesReportAggregatesSnapshot>>
{
    /// <summary>Interval start (UTC, inclusive).</summary>
    public DateTimeOffset PeriodStartUtc { get; }

    /// <summary>Interval end (UTC, exclusive).</summary>
    public DateTimeOffset PeriodEndUtc { get; }

    /// <summary>Creates a sales report aggregation request.</summary>
    public GetSalesReportAggregatesRequest(DateTimeOffset periodStartUtc, DateTimeOffset periodEndUtc)
    {
        PeriodStartUtc = periodStartUtc;
        PeriodEndUtc = periodEndUtc;
    }
}
