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

public class ListTutorPetTimelineQueryHandlerTests
{
    [Fact]
    public async Task Handle_MergesClinicalAndGrooming_WithoutClinicalNotes()
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
        readPort.ListTimelineAsync(petId, Arg.Any<CancellationToken>()).Returns(new List<TutorPetTimelineItemDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                SourceType = "Appointment",
                Title = "Consulta",
                Status = "Completed",
                OccurredAt = DateTimeOffset.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                SourceType = "Grooming",
                Title = "Banho",
                Status = "Completed",
                OccurredAt = DateTimeOffset.UtcNow
            }
        });

        var handler = new ListTutorPetTimelineQueryHandler(
            new TutorPortalUserResolver(currentUser, Substitute.For<ITutorPortalAccountRepository>()),
            new TutorPetAccessGuard(petRepository),
            readPort);

        var result = await handler.Handle(new ListTutorPetTimelineQuery(petId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(i => i.Title).Should().BeEquivalentTo(new[] { "Consulta", "Banho" });
        result.Value.Should().NotContain(i => i.Title.Contains("Anamnesis", StringComparison.OrdinalIgnoreCase));
    }
}
