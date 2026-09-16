using Core.Application.Pets.Commands;
using Core.Domain;
using Core.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.Pets;

public class UpdatePetCommandHandlerTests
{
    private readonly IPetRepository _petRepository;
    private readonly UpdatePetCommandHandler _handler;

    public UpdatePetCommandHandlerTests()
    {
        _petRepository = Substitute.For<IPetRepository>();
        _handler = new UpdatePetCommandHandler(_petRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateAndReturnSuccess()
    {
        var id = Guid.NewGuid();
        var petResult = Pet.Create("Rex", PetSpecies.Dog, "Poodle", PetSex.Male, Guid.NewGuid(), id);
        _petRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(petResult.Value);

        var command = new UpdatePetCommand(id, "Rex 2", PetSpecies.Dog, "Bulldog", PetSex.Male);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _petRepository.Received(1).Update(Arg.Any<Pet>());
    }

    [Fact]
    public async Task Handle_WithNonExistingId_ShouldReturnFailure()
    {
        var id = Guid.NewGuid();
        _petRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Pet?)null);

        var command = new UpdatePetCommand(id, "Rex 2", PetSpecies.Dog, "Bulldog", PetSex.Male);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Pet.NotFound");
        _petRepository.DidNotReceive().Update(Arg.Any<Pet>());
    }
}
