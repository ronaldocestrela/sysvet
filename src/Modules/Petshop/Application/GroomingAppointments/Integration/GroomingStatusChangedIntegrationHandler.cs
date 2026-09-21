using Core.Application.Common.Interfaces;
using Core.Application.IntegrationEvents;
using MediatR;
using Petshop.Domain.Enums;
using Petshop.Domain.Events;

namespace Petshop.Application.GroomingAppointments.Integration;

/// <summary>
/// Maps grooming domain events to cross-module integration notifications.
/// </summary>
public sealed class GroomingStatusChangedIntegrationHandler : INotificationHandler<DomainEventEnvelope>
{
    private readonly IPublisher _publisher;
    private readonly ICurrentUser _currentUser;

    public GroomingStatusChangedIntegrationHandler(IPublisher publisher, ICurrentUser currentUser)
    {
        _publisher = publisher;
        _currentUser = currentUser;
    }

    public async Task Handle(DomainEventEnvelope notification, CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;
        if (tenantId == Guid.Empty)
        {
            return;
        }

        switch (notification.DomainEvent)
        {
            case GroomingStartedDomainEvent started:
                await PublishAsync(
                    started.GroomingAppointmentId,
                    started.PetId,
                    started.TutorId,
                    GroomingNotificationKind.Started,
                    GroomingAppointmentStatus.InProgress.ToString(),
                    started.OccurredOn,
                    cancellationToken);
                break;
            case GroomingReadyForPickupDomainEvent ready:
                await PublishAsync(
                    ready.GroomingAppointmentId,
                    ready.PetId,
                    ready.TutorId,
                    GroomingNotificationKind.ReadyForPickup,
                    GroomingAppointmentStatus.ReadyForPickup.ToString(),
                    ready.OccurredOn,
                    cancellationToken);
                break;
            case GroomingCompletedDomainEvent completed:
                await PublishAsync(
                    completed.GroomingAppointmentId,
                    completed.PetId,
                    completed.TutorId,
                    GroomingNotificationKind.Completed,
                    GroomingAppointmentStatus.Completed.ToString(),
                    completed.OccurredOn,
                    cancellationToken);
                break;
        }
    }

    private Task PublishAsync(
        Guid appointmentId,
        Guid petId,
        Guid tutorId,
        GroomingNotificationKind kind,
        string status,
        DateTimeOffset occurredOn,
        CancellationToken cancellationToken) =>
        _publisher.Publish(
            new GroomingStatusChangedEvent(
                _currentUser.TenantId,
                appointmentId,
                petId,
                tutorId,
                kind,
                status,
                occurredOn),
            cancellationToken);
}
