namespace Automations.Application.Abstractions;

/// <summary>
/// Cross-module read model for completed clinical and grooming visits (Fase 8.3).
/// </summary>
public interface ITutorVisitReadPort
{
    /// <summary>
    /// Returns the latest completed visit instant per active tutor, when any exists.
    /// </summary>
    Task<IReadOnlyList<TutorLastVisitRow>> ListLastCompletedVisitsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns completed services whose completion timestamp falls in the UTC half-open interval.
    /// </summary>
    Task<IReadOnlyList<CompletedServiceRow>> ListCompletedBetweenAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtcExclusive,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Aggregates visit counts and spacing for return-frequency reporting.
    /// </summary>
    Task<IReadOnlyList<TutorReturnFrequencyRow>> GetReturnFrequencyAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Last visit projection for a tutor.
/// </summary>
public sealed record TutorLastVisitRow(Guid TutorId, DateTimeOffset LastVisitAt);

/// <summary>
/// A single completed service used for NPS triggers.
/// </summary>
public sealed record CompletedServiceRow(
    Guid TutorId,
    Guid PetId,
    Guid SourceId,
    string SourceType,
    DateTimeOffset ServiceAt);

/// <summary>
/// Return-frequency metrics for reporting.
/// </summary>
public sealed record TutorReturnFrequencyRow(
    Guid TutorId,
    int VisitCount,
    DateTimeOffset? LastVisitAt,
    double? AverageDaysBetweenVisits);
