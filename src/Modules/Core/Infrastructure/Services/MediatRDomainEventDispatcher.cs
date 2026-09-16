using Core.Application.Common.Interfaces;
using Core.Application.IntegrationEvents;
using Core.Domain;
using MediatR;

namespace Core.Infrastructure.Services;

/// <summary>
/// Publishes domain events through MediatR as <see cref="DomainEventEnvelope"/> notifications.
/// </summary>
public class MediatRDomainEventDispatcher(IPublisher publisher) : IDomainEventDispatcher
{
    public Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default) =>
        publisher.Publish(new DomainEventEnvelope(domainEvent), cancellationToken);
}
