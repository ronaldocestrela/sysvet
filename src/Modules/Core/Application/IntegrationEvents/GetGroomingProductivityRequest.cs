using Core.Domain;
using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>Counts completed grooming appointments by groomer (Petshop module).</summary>
public sealed class GetGroomingProductivityRequest : IRequest<Result<GroomingProductivitySnapshot>>
{
    /// <summary>Interval start (UTC, inclusive).</summary>
    public DateTimeOffset PeriodStartUtc { get; }

    /// <summary>Interval end (UTC, exclusive).</summary>
    public DateTimeOffset PeriodEndUtc { get; }

    /// <summary>Creates a grooming productivity request.</summary>
    public GetGroomingProductivityRequest(DateTimeOffset periodStartUtc, DateTimeOffset periodEndUtc)
    {
        PeriodStartUtc = periodStartUtc;
        PeriodEndUtc = periodEndUtc;
    }
}
