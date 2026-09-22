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

public class ListTutorPetExamsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsExams_WhenPetAuthorized()
    {
        var tutorId = Guid.NewGuid();
        var petId = Guid.NewGuid();
        var pet = Pet.Create("Rex", PetSpecies.Dog, "SRD", PetSex.Male, tutorId, petId).Value;

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.IsAuthenticated.Returns(true);
        currentUser.UserId.Returns("user-1");
        currentUser.TutorId.Returns(tutorId);

        var petRepository = Substitute.For<IPetRepository>();
        petRepository.GetByIdAsync(petId, Arg.Any<CancellationToken>()).Returns(pet);

        var readPort = Substitute.For<ITutorPetHealthReadPort>();
        readPort.ListExamsAsync(petId, Arg.Any<CancellationToken>()).Returns(new List<TutorPetExamDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Hemograma", Status = "Completed" }
        });

        var handler = new ListTutorPetExamsQueryHandler(
            new TutorPortalUserResolver(currentUser, Substitute.For<ITutorPortalAccountRepository>()),
            new TutorPetAccessGuard(petRepository),
            readPort);

        var result = await handler.Handle(new ListTutorPetExamsQuery(petId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(e => e.Name == "Hemograma");
    }
}
