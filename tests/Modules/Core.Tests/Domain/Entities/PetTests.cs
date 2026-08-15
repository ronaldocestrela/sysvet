using FluentAssertions;
using Core.Domain.Entities;

namespace Core.Tests.Domain.Entities;

public class PetTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenPetDataIsValid()
    {
        // Arrange
        var tutorId = Guid.NewGuid();

        // Act
        var result = Pet.Create("Thor", PetSpecies.Dog, "Bulldog", PetSex.Male, tutorId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Thor");
        result.Value.Species.Should().Be(PetSpecies.Dog);
        result.Value.Breed.Should().Be("Bulldog");
        result.Value.Sex.Should().Be(PetSex.Male);
        result.Value.TutorId.Should().Be(tutorId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldReturnFailure_WhenPetNameIsEmpty(string invalidName)
    {
        // Arrange
        var tutorId = Guid.NewGuid();

        // Act
        var result = Pet.Create(invalidName, PetSpecies.Cat, "Persan", PetSex.Female, tutorId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Pet.InvalidName");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenTutorIdIsEmpty()
    {
        // Act
        var result = Pet.Create("Thor", PetSpecies.Dog, "Bulldog", PetSex.Male, Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Pet.InvalidTutor");
    }
}
