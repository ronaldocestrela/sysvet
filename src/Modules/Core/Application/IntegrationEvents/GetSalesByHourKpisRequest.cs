using Core.Domain;
using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>Aggregates hourly paid POS sales for a UTC half-open interval (Sales module).</summary>
public sealed class GetSalesByHourKpisRequest : IRequest<Result<SalesByHourKpiSnapshot>>
{
    /// <summary>Interval start (UTC, inclusive).</summary>
    public DateTimeOffset PeriodStartUtc { get; }

    /// <summary>Interval end (UTC, exclusive).</summary>
    public DateTimeOffset PeriodEndUtc { get; }

    /// <summary>IANA timezone used to bucket hours (e.g. America/Sao_Paulo).</summary>
    public string TimeZoneId { get; }

    /// <summary>Creates an hourly sales KPI request.</summary>
    public GetSalesByHourKpisRequest(DateTimeOffset periodStartUtc, DateTimeOffset periodEndUtc, string timeZoneId)
    {
        PeriodStartUtc = periodStartUtc;
        PeriodEndUtc = periodEndUtc;
        TimeZoneId = timeZoneId;
    }
}
