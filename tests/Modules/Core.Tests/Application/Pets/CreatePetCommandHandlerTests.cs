using Core.Application.Pets.Commands;
using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.Pets;

public class CreatePetCommandHandlerTests
{
    private readonly IPetRepository _petRepository;
    private readonly ITutorRepository _tutorRepository;
    private readonly CreatePetCommandHandler _handler;

    public CreatePetCommandHandlerTests()
    {
        _petRepository = Substitute.For<IPetRepository>();
        _tutorRepository = Substitute.For<ITutorRepository>();
        _handler = new CreatePetCommandHandler(_petRepository, _tutorRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        var tutorId = Guid.NewGuid();
        var tutor = Tutor.Create("John Doe", Email.Create("john@example.com").Value, Cpf.Create("12345678909").Value, Phone.Create("11999999999").Value, tutorId).Value;

        _tutorRepository.GetByIdAsync(tutorId, Arg.Any<CancellationToken>()).Returns(tutor);

        var command = new CreatePetCommand("Rex", PetSpecies.Dog, "Poodle", PetSex.Male, tutorId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _petRepository.Received(1).Add(Arg.Any<Pet>());
    }

    [Fact]
    public async Task Handle_WithInactiveTutor_ShouldReturnFailure()
    {
        var tutorId = Guid.NewGuid();
        var tutor = Tutor.Create("John Doe", Email.Create("john@example.com").Value, Cpf.Create("12345678909").Value, Phone.Create("11999999999").Value, tutorId).Value;
        tutor.SoftDelete();

        _tutorRepository.GetByIdAsync(tutorId, Arg.Any<CancellationToken>()).Returns(tutor);

        var command = new CreatePetCommand("Rex", PetSpecies.Dog, "Poodle", PetSex.Male, tutorId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Pet.TutorInactive");
    }

    [Fact]
    public async Task Handle_WithNonExistingTutor_ShouldReturnFailure()
    {
        var tutorId = Guid.NewGuid();
        _tutorRepository.GetByIdAsync(tutorId, Arg.Any<CancellationToken>()).Returns((Tutor?)null);

        var command = new CreatePetCommand("Rex", PetSpecies.Dog, "Poodle", PetSex.Male, tutorId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Pet.TutorNotFound");
        _petRepository.DidNotReceive().Add(Arg.Any<Pet>());
    }
}
