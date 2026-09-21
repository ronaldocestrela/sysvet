using Core.Application.IntegrationEvents;
using Core.Application.Notifications;
using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using GroomingTutorNotificationHandler = Core.Application.Notifications.GroomingTutorNotificationHandler;

namespace Petshop.Tests.Application;

public class GroomingTutorNotificationHandlerTests
{
    [Fact]
    public async Task Handle_WhenChannelDisabled_ShouldNotNotify()
    {
        var channel = Substitute.For<ITutorNotificationChannel>();
        channel.IsEnabled.Returns(false);
        var handler = CreateHandler(channel);

        await handler.Handle(CreateReadyEvent(), CancellationToken.None);

        await channel.DidNotReceive().NotifyGroomingStatusAsync(Arg.Any<TutorGroomingNotification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenChannelEnabledAndReady_ShouldNotifyTutor()
    {
        var tutorId = Guid.NewGuid();
        var petId = Guid.NewGuid();
        var channel = Substitute.For<ITutorNotificationChannel>();
        channel.IsEnabled.Returns(true);

        var tutorRepo = Substitute.For<ITutorRepository>();
        tutorRepo.GetByIdAsync(tutorId, Arg.Any<CancellationToken>())
            .Returns(Tutor.Create("Maria", Email.Create("a@b.com").Value, Cpf.Create("52998224725").Value, Phone.Create("11999998888").Value, tutorId).Value);

        var petRepo = Substitute.For<IPetRepository>();
        petRepo.GetByIdAsync(petId, Arg.Any<CancellationToken>())
            .Returns(Pet.Create("Rex", PetSpecies.Dog, "SRD", PetSex.Male, tutorId, petId).Value);

        var handler = CreateHandler(channel, tutorRepo, petRepo);
        var evt = CreateReadyEvent(tutorId, petId);

        await handler.Handle(evt, CancellationToken.None);

        await channel.Received(1).NotifyGroomingStatusAsync(
            Arg.Is<TutorGroomingNotification>(n =>
                n.TutorId == tutorId &&
                n.Kind == GroomingNotificationKind.ReadyForPickup),
            Arg.Any<CancellationToken>());
    }

    private static GroomingTutorNotificationHandler CreateHandler(
        ITutorNotificationChannel channel,
        ITutorRepository? tutorRepository = null,
        IPetRepository? petRepository = null) =>
        new(
            channel,
            tutorRepository ?? Substitute.For<ITutorRepository>(),
            petRepository ?? Substitute.For<IPetRepository>(),
            NullLogger<GroomingTutorNotificationHandler>.Instance);

    private static GroomingStatusChangedEvent CreateReadyEvent(Guid? tutorId = null, Guid? petId = null) =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            petId ?? Guid.NewGuid(),
            tutorId ?? Guid.NewGuid(),
            GroomingNotificationKind.ReadyForPickup,
            "ReadyForPickup",
            DateTimeOffset.UtcNow);
}
