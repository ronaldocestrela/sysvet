using Core.Domain;
using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>
/// MediatR notification wrapper so domain events can be handled without referencing MediatR from Domain.
/// </summary>
public sealed record DomainEventEnvelope(IDomainEvent DomainEvent) : INotification;
