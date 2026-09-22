using Core.Application.Common.Interfaces;
using Core.Domain;
using Core.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using TutorPortal.Application.Abstractions;
using TutorPortal.Application.Auth;
using TutorPortal.Application.PetHealth;
using TutorPortal.Application.Scheduling;
using TutorPortal.Application.Scheduling.Dtos;
using TutorPortal.Application.Scheduling.Queries;
using TutorPortal.Domain.Repositories;

namespace TutorPortal.Tests.Application;

public class TutorBookingQueryHandlerTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly ITutorPortalAccountRepository _accountRepository = Substitute.For<ITutorPortalAccountRepository>();
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly ITutorSchedulingPort _schedulingPort = Substitute.For<ITutorSchedulingPort>();

    [Fact]
    public async Task ListServices_ReturnsClinicalAndGrooming_WhenPetOwned()
    {
        var tutorId = Guid.NewGuid();
        var petId = Guid.NewGuid();
        SetupOwnedPet(tutorId, petId);

        _schedulingPort.ListServicesAsync(Arg.Any<CancellationToken>()).Returns(new List<TutorBookableServiceDto>
        {
            new() { ServiceId = TutorBookingConstants.ClinicalConsultationServiceId, Name = "Consulta clínica", Kind = "Clinical" },
            new() { ServiceId = Guid.NewGuid(), Name = "Banho", Kind = "Grooming" }
        });

        var handler = new ListTutorBookableServicesQueryHandler(CreateResolver(), CreateGuard(), _schedulingPort);
        var result = await handler.Handle(new ListTutorBookableServicesQuery(petId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task ListAvailableSlots_DelegatesToPort_WhenPetOwned()
    {
        var tutorId = Guid.NewGuid();
        var petId = Guid.NewGuid();
        SetupOwnedPet(tutorId, petId);

        var start = DateTimeOffset.UtcNow.AddDays(2);
        _schedulingPort.ListAvailableSlotsAsync(
                TutorBookingKind.Clinical,
                TutorBookingConstants.ClinicalConsultationServiceId,
                Arg.Any<Guid>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<CancellationToken>())
            .Returns(new List<TutorAvailableSlotDto> { new() { Start = start, DurationInMinutes = 30 } });

        var handler = new ListTutorAvailableSlotsQueryHandler(CreateResolver(), CreateGuard(), _schedulingPort);
        var result = await handler.Handle(
            new ListTutorAvailableSlotsQuery(
                petId,
                TutorBookingKind.Clinical,
                TutorBookingConstants.ClinicalConsultationServiceId,
                Guid.NewGuid(),
                start.Date),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(s => s.DurationInMinutes == 30);
    }

    [Fact]
    public async Task ListServices_Fails_WhenPetNotOwned()
    {
        var tutorId = Guid.NewGuid();
        var petId = Guid.NewGuid();
        var pet = Pet.Create("Rex", PetSpecies.Dog, "SRD", PetSex.Male, Guid.NewGuid(), petId).Value;
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns("user-1");
        _currentUser.TutorId.Returns(tutorId);
        _petRepository.GetByIdAsync(petId, Arg.Any<CancellationToken>()).Returns(pet);

        var handler = new ListTutorBookableServicesQueryHandler(CreateResolver(), CreateGuard(), _schedulingPort);
        var result = await handler.Handle(new ListTutorBookableServicesQuery(petId), CancellationToken.None);

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
