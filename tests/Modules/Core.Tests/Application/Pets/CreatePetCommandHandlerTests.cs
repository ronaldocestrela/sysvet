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
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreatePetCommandHandler _handler;

    public CreatePetCommandHandlerTests()
    {
        _petRepository = Substitute.For<IPetRepository>();
        _tutorRepository = Substitute.For<ITutorRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CreatePetCommandHandler(_petRepository, _tutorRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var tutorId = Guid.NewGuid();
        var tutor = Tutor.Create("John Doe", Email.Create("john@example.com").Value, Cpf.Create("12345678909").Value, Phone.Create("11999999999").Value, tutorId).Value;
        
        _tutorRepository.GetByIdAsync(tutorId, Arg.Any<CancellationToken>()).Returns(tutor);

        var command = new CreatePetCommand("Rex", PetSpecies.Dog, "Poodle", PetSex.Male, tutorId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _petRepository.Received(1).Add(Arg.Any<Pet>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistingTutor_ShouldReturnFailure()
    {
        // Arrange
        var tutorId = Guid.NewGuid();
        _tutorRepository.GetByIdAsync(tutorId, Arg.Any<CancellationToken>()).Returns((Tutor?)null);

        var command = new CreatePetCommand("Rex", PetSpecies.Dog, "Poodle", PetSex.Male, tutorId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Pet.TutorNotFound");
        _petRepository.DidNotReceive().Add(Arg.Any<Pet>());
    }
}
