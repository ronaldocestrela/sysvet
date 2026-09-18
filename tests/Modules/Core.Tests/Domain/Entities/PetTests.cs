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

    [Fact]
    public void Create_ShouldReturnFailure_WhenSpeciesIsUndefined()
    {
        var tutorId = Guid.NewGuid();

        var result = Pet.Create("Thor", (PetSpecies)0, "Bulldog", PetSex.Male, tutorId);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Pet.InvalidSpecies");
    }

    [Fact]
    public void SoftDelete_ShouldMarkAsDeleted()
    {
        var tutorId = Guid.NewGuid();
        var pet = Pet.Create("Thor", PetSpecies.Dog, "Bulldog", PetSex.Male, tutorId).Value;

        var result = pet.SoftDelete();

        result.IsSuccess.Should().BeTrue();
        pet.IsDeleted.Should().BeTrue();
        pet.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_ShouldReturnFailure_WhenPetIsDeleted()
    {
        var tutorId = Guid.NewGuid();
        var pet = Pet.Create("Thor", PetSpecies.Dog, "Bulldog", PetSex.Male, tutorId).Value;
        pet.SoftDelete();

        var result = pet.Update("New Name", PetSpecies.Cat, "Persa", PetSex.Female);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Pet.AlreadyDeleted");
    }

    [Fact]
    public void Update_ShouldBumpUpdatedAt_WhenSuccessful()
    {
        var tutorId = Guid.NewGuid();
        var pet = Pet.Create("Thor", PetSpecies.Dog, "Bulldog", PetSex.Male, tutorId).Value;
        var before = pet.UpdatedAt;
        Thread.Sleep(5);

        var result = pet.Update("Thor Jr", PetSpecies.Dog, "Bulldog", PetSex.Male);

        result.IsSuccess.Should().BeTrue();
        pet.UpdatedAt.Should().BeAfter(before);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenBirthDateIsInFuture()
    {
        var tutorId = Guid.NewGuid();
        var future = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));

        var result = Pet.Create("Thor", PetSpecies.Dog, "Bulldog", PetSex.Male, tutorId, birthDate: future);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Pet.InvalidBirthDate");
    }

    [Fact]
    public void Create_ShouldPersistBirthDate_WhenValid()
    {
        var tutorId = Guid.NewGuid();
        var birth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2));

        var result = Pet.Create("Thor", PetSpecies.Dog, "Bulldog", PetSex.Male, tutorId, birthDate: birth);

        result.IsSuccess.Should().BeTrue();
        result.Value.BirthDate.Should().Be(birth);
    }

    [Fact]
    public void SoftDelete_ShouldBumpUpdatedAt_WhenFirstDelete()
    {
        var tutorId = Guid.NewGuid();
        var pet = Pet.Create("Thor", PetSpecies.Dog, "Bulldog", PetSex.Male, tutorId).Value;
        var before = pet.UpdatedAt;
        Thread.Sleep(5);

        pet.SoftDelete().IsSuccess.Should().BeTrue();

        pet.UpdatedAt.Should().BeAfter(before);
    }
}
