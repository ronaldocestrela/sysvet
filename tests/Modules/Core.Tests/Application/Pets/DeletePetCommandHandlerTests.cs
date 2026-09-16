using Core.Application.Pets.Commands;
using Core.Domain;
using Core.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.Pets;

public class DeletePetCommandHandlerTests
{
    private readonly IPetRepository _petRepository;
    private readonly DeletePetCommandHandler _handler;

    public DeletePetCommandHandlerTests()
    {
        _petRepository = Substitute.For<IPetRepository>();
        _handler = new DeletePetCommandHandler(_petRepository);
    }

    [Fact]
    public async Task Handle_ShouldSoftDeletePet()
    {
        var petId = Guid.NewGuid();
        var tutorId = Guid.NewGuid();
        var pet = Pet.Create("Rex", PetSpecies.Dog, "Poodle", PetSex.Male, tutorId, petId).Value;
        _petRepository.GetByIdAsync(petId, Arg.Any<CancellationToken>()).Returns(pet);

        var result = await _handler.Handle(new DeletePetCommand(petId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        pet.IsDeleted.Should().BeTrue();
    }
}
