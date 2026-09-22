using Core.Application.Common.Interfaces;
using Core.Domain;
using Core.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using TutorPortal.Application.Abstractions;
using TutorPortal.Application.Auth;
using TutorPortal.Application.PetHealth;
using TutorPortal.Application.Scheduling;
using TutorPortal.Application.Scheduling.Commands;
using TutorPortal.Domain.Repositories;

namespace TutorPortal.Tests.Application;

public class TutorBookingCommandHandlerTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly ITutorPortalAccountRepository _accountRepository = Substitute.For<ITutorPortalAccountRepository>();
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly ITutorSchedulingPort _schedulingPort = Substitute.For<ITutorSchedulingPort>();

    [Fact]
    public async Task Book_CallsPortWithTutorId_WhenPetOwned()
    {
        var tutorId = Guid.NewGuid();
        var petId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var date = DateTimeOffset.UtcNow.AddDays(3);
        SetupOwnedPet(tutorId, petId);

        _schedulingPort.BookAsync(
                appointmentId,
                tutorId,
                petId,
                TutorBookingKind.Clinical,
                TutorBookingConstants.ClinicalConsultationServiceId,
                professionalId,
                date,
                Arg.Any<CancellationToken>())
            .Returns(Result.Success(appointmentId));

        var handler = new BookTutorAppointmentCommandHandler(CreateResolver(), CreateGuard(), _schedulingPort);
        var result = await handler.Handle(
            new BookTutorAppointmentCommand(
                petId,
                TutorBookingKind.Clinical,
                TutorBookingConstants.ClinicalConsultationServiceId,
                professionalId,
                date,
                appointmentId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(appointmentId);
    }

    [Fact]
    public async Task Cancel_CallsPort_WhenPetOwned()
    {
        var tutorId = Guid.NewGuid();
        var petId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        SetupOwnedPet(tutorId, petId);

        _schedulingPort.CancelAsync(
                tutorId,
                petId,
                TutorBookingKind.Clinical,
                appointmentId,
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var handler = new CancelTutorAppointmentCommandHandler(CreateResolver(), CreateGuard(), _schedulingPort);
        var result = await handler.Handle(
            new CancelTutorAppointmentCommand(petId, TutorBookingKind.Clinical, appointmentId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Cancel_Fails_WhenPetNotOwned()
    {
        var tutorId = Guid.NewGuid();
        var petId = Guid.NewGuid();
        var pet = Pet.Create("Rex", PetSpecies.Dog, "SRD", PetSex.Male, Guid.NewGuid(), petId).Value;
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns("user-1");
        _currentUser.TutorId.Returns(tutorId);
        _petRepository.GetByIdAsync(petId, Arg.Any<CancellationToken>()).Returns(pet);

        var handler = new CancelTutorAppointmentCommandHandler(CreateResolver(), CreateGuard(), _schedulingPort);
        var result = await handler.Handle(
            new CancelTutorAppointmentCommand(petId, TutorBookingKind.Clinical, Guid.NewGuid()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TutorPortal.Pet.NotFound");
    }

    private void SetupOwnedPet(Guid tutorId, Guid petId)
    {
        var pet = Pet.Create("Rex", PetSpecies.Dog, "SRD", PetSex.Male, tutorId, petId).Value;
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns("user-1");
        _currentUser.TutorId.Returns(tutorId);
        _petRepository.GetByIdAsync(petId, Arg.Any<CancellationToken>()).Returns(pet);
    }

    private TutorPortalUserResolver CreateResolver() => new(_currentUser, _accountRepository);

    private TutorPetAccessGuard CreateGuard() => new(_petRepository);
}
