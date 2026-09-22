using Core.Application.Common.Interfaces;
using Core.Domain;
using Core.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using TutorPortal.Application.Abstractions;
using TutorPortal.Application.Auth;
using TutorPortal.Application.PetHealth;
using TutorPortal.Application.PetHealth.Dtos;
using TutorPortal.Application.PetHealth.Queries;
using TutorPortal.Domain.Repositories;

namespace TutorPortal.Tests.Application;

public class GetTutorVaccinationCardQueryHandlerTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly ITutorPortalAccountRepository _accountRepository = Substitute.For<ITutorPortalAccountRepository>();
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly ITutorPetHealthReadPort _readPort = Substitute.For<ITutorPetHealthReadPort>();

    [Fact]
    public async Task Handle_ReturnsCard_WhenPetBelongsToTutor()
    {
        var tutorId = Guid.NewGuid();
        var petId = Guid.NewGuid();
        var pet = Pet.Create("Rex", PetSpecies.Dog, "SRD", PetSex.Male, tutorId, petId).Value;

        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns("user-1");
        _currentUser.TutorId.Returns(tutorId);
        _petRepository.GetByIdAsync(petId, Arg.Any<CancellationToken>()).Returns(pet);

        var card = new TutorVaccinationCardDto { PetId = petId, PetName = "Rex" };
        _readPort.GetVaccinationCardAsync(petId, Arg.Any<CancellationToken>()).Returns(card);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetTutorVaccinationCardQuery(petId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PetName.Should().Be("Rex");
    }

    [Fact]
    public async Task Handle_Fails_WhenPetNotOwned()
    {
        var tutorId = Guid.NewGuid();
        var otherTutor = Guid.NewGuid();
        var petId = Guid.NewGuid();
        var pet = Pet.Create("Rex", PetSpecies.Dog, "SRD", PetSex.Male, otherTutor, petId).Value;

        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns("user-1");
        _currentUser.TutorId.Returns(tutorId);
        _petRepository.GetByIdAsync(petId, Arg.Any<CancellationToken>()).Returns(pet);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetTutorVaccinationCardQuery(petId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TutorPortal.Pet.NotFound");
    }

    private GetTutorVaccinationCardQueryHandler CreateHandler()
    {
        var resolver = new TutorPortalUserResolver(_currentUser, _accountRepository);
        var guard = new TutorPetAccessGuard(_petRepository);
        return new GetTutorVaccinationCardQueryHandler(resolver, guard, _readPort);
    }
}
