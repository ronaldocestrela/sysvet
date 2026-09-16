namespace Core.Domain;

/// <summary>
/// Exposes aggregate roots with pending domain events from a persistence context after changes are tracked.
/// </summary>
public interface IDomainEventSource
{
    /// <summary>
    /// Aggregate roots that raised domain events during the current unit of work.
    /// </summary>
    IReadOnlyCollection<AggregateRoot> GetAggregateRootsWithPendingEvents();
}
