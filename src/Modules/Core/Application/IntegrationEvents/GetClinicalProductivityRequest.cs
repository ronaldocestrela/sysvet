using Core.Domain;
using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>Counts completed clinical appointments by veterinarian (Veterinary module).</summary>
public sealed class GetClinicalProductivityRequest : IRequest<Result<ClinicalProductivitySnapshot>>
{
    /// <summary>Interval start (UTC, inclusive).</summary>
    public DateTimeOffset PeriodStartUtc { get; }

    /// <summary>Interval end (UTC, exclusive).</summary>
    public DateTimeOffset PeriodEndUtc { get; }

    /// <summary>Creates a clinical productivity request.</summary>
    public GetClinicalProductivityRequest(DateTimeOffset periodStartUtc, DateTimeOffset periodEndUtc)
    {
        PeriodStartUtc = periodStartUtc;
        PeriodEndUtc = periodEndUtc;
    }
}
