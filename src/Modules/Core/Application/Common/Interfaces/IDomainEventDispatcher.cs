using Core.Domain;

namespace Core.Application.Common.Interfaces;

/// <summary>
/// Dispatches domain events raised by aggregates after successful persistence.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Publishes a single domain event to in-process handlers.
    /// </summary>
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
