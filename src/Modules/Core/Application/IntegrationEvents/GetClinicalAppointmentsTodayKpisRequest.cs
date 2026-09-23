using Core.Domain;
using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>Counts clinical appointments on the business day (Veterinary module).</summary>
public sealed class GetClinicalAppointmentsTodayKpisRequest : IRequest<Result<ClinicalAppointmentsTodayKpiSnapshot>>
{
    /// <summary>Interval start (UTC, inclusive).</summary>
    public DateTimeOffset PeriodStartUtc { get; }

    /// <summary>Interval end (UTC, exclusive).</summary>
    public DateTimeOffset PeriodEndUtc { get; }

    /// <summary>Creates a clinical appointments KPI request.</summary>
    public GetClinicalAppointmentsTodayKpisRequest(DateTimeOffset periodStartUtc, DateTimeOffset periodEndUtc)
    {
        PeriodStartUtc = periodStartUtc;
        PeriodEndUtc = periodEndUtc;
    }
}
