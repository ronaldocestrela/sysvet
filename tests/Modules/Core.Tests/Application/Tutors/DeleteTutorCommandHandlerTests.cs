using Core.Application.Tutors.Commands;
using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.Tutors;

public class DeleteTutorCommandHandlerTests
{
    private readonly ITutorRepository _tutorRepository;
    private readonly IPetRepository _petRepository;
    private readonly DeleteTutorCommandHandler _handler;

    public DeleteTutorCommandHandlerTests()
    {
        _tutorRepository = Substitute.For<ITutorRepository>();
        _petRepository = Substitute.For<IPetRepository>();
        _handler = new DeleteTutorCommandHandler(_tutorRepository, _petRepository);
    }

    [Fact]
    public async Task Handle_ShouldSoftDeleteTutorAndPets()
    {
        var tutorId = Guid.NewGuid();
        var tutor = Tutor.Create("John", Email.Create("j@e.com").Value, Cpf.Create("12345678909").Value, Phone.Create("11999999999").Value, tutorId).Value;
        var pet = Pet.Create("Rex", PetSpecies.Dog, "Poodle", PetSex.Male, tutorId).Value;

        _tutorRepository.GetByIdAsync(tutorId, Arg.Any<CancellationToken>()).Returns(tutor);
        _petRepository.GetByTutorIdAsync(tutorId, Arg.Any<CancellationToken>()).Returns(new[] { pet });

        var result = await _handler.Handle(new DeleteTutorCommand(tutorId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tutor.IsDeleted.Should().BeTrue();
        pet.IsDeleted.Should().BeTrue();
    }
}
