using Core.Application.Common.Interfaces;
using Core.Application.IntegrationEvents;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Petshop.Application.GroomingAppointments.Integration;
using Petshop.Domain.Events;
using Xunit;

namespace Petshop.Tests.Application;

public class GroomingStatusChangedIntegrationHandlerTests
{
    [Fact]
    public async Task Handle_WhenReadyForPickup_ShouldPublishGroomingStatusChangedEvent()
    {
        var tenantId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var petId = Guid.NewGuid();
        var tutorId = Guid.NewGuid();
        var publisher = Substitute.For<IPublisher>();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.TenantId.Returns(tenantId);

        var handler = new GroomingStatusChangedIntegrationHandler(publisher, currentUser);
        var occurredOn = DateTimeOffset.UtcNow;
        var envelope = new DomainEventEnvelope(
            new GroomingReadyForPickupDomainEvent(appointmentId, petId, tutorId, occurredOn));

        await handler.Handle(envelope, CancellationToken.None);

        await publisher.Received(1).Publish(
            Arg.Is<GroomingStatusChangedEvent>(e =>
                e.TenantId == tenantId &&
                e.GroomingAppointmentId == appointmentId &&
                e.Kind == GroomingNotificationKind.ReadyForPickup &&
                e.Status == "ReadyForPickup"),
            Arg.Any<CancellationToken>());
    }
}
