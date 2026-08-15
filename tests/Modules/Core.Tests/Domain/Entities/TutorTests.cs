using FluentAssertions;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;

namespace Core.Tests.Domain.Entities;

public class TutorTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenDataIsValid()
    {
        // Arrange
        var name = "Maria Silva";
        var email = Email.Create("maria@example.com").Value;
        var cpf = Cpf.Create("12345678909").Value;
        var phone = Phone.Create("11999998888").Value;

        // Act
        var result = Tutor.Create(name, email, cpf, phone);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
        result.Value.Email.Should().Be(email);
        result.Value.Cpf.Should().Be(cpf);
        result.Value.Phone.Should().Be(phone);
        result.Value.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public void Create_ShouldReturnFailure_WhenNameIsInvalid(string invalidName)
    {
        // Arrange
        var email = Email.Create("maria@example.com").Value;
        var cpf = Cpf.Create("12345678909").Value;
        var phone = Phone.Create("11999998888").Value;

        // Act
        var result = Tutor.Create(invalidName, email, cpf, phone);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Tutor.InvalidName");
    }

    [Fact]
    public void AddPet_ShouldAddPetToTutorList()
    {
        // Arrange
        var tutor = Tutor.Create("Maria Silva", 
            Email.Create("maria@example.com").Value, 
            Cpf.Create("12345678909").Value, 
            Phone.Create("11999998888").Value).Value;

        var petResult = Pet.Create("Rex", PetSpecies.Dog, "Golden Retriever", PetSex.Male, tutor.Id);
        petResult.IsSuccess.Should().BeTrue();

        // Act
        var addResult = tutor.AddPet(petResult.Value);

        // Assert
        addResult.IsSuccess.Should().BeTrue();
        tutor.Pets.Should().ContainSingle(p => p.Name == "Rex");
    }
}
