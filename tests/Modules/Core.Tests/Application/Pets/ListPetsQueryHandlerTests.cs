using Core.Application.Pets.Queries;
using Core.Domain;
using Core.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.Pets;

public class ListPetsQueryHandlerTests
{
    private readonly IPetRepository _petRepository;
    private readonly ListPetsQueryHandler _handler;

    public ListPetsQueryHandlerTests()
    {
        _petRepository = Substitute.For<IPetRepository>();
        _handler = new ListPetsQueryHandler(_petRepository);
    }

    [Fact]
    public async Task Handle_WithTutorFilter_ShouldReturnFilteredPets()
    {
        // Arrange
        var tutorId = Guid.NewGuid();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        
        var pet1 = Pet.Create("Rex", PetSpecies.Dog, "Poodle", PetSex.Male, tutorId, id1).Value;
        var pet2 = Pet.Create("Miau", PetSpecies.Cat, "Persa", PetSex.Female, Guid.NewGuid(), id2).Value;

        _petRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Pet> { pet1, pet2 });

        var query = new ListPetsQuery(TutorId: tutorId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Name.Should().Be("Rex");
    }
}
