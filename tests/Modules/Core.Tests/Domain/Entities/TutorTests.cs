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

    [Fact]
    public void AddPet_ShouldReturnFailure_WhenTutorIsInactive()
    {
        var tutor = Tutor.Create("Maria Silva",
            Email.Create("maria@example.com").Value,
            Cpf.Create("12345678909").Value,
            Phone.Create("11999998888").Value).Value;
        tutor.SoftDelete();

        var pet = Pet.Create("Rex", PetSpecies.Dog, "Golden Retriever", PetSex.Male, tutor.Id).Value;

        var addResult = tutor.AddPet(pet);

        addResult.IsFailure.Should().BeTrue();
        addResult.Error.Code.Should().Be("Pet.TutorInactive");
    }

    [Fact]
    public void SoftDelete_ShouldBeIdempotent()
    {
        var tutor = Tutor.Create("Maria Silva",
            Email.Create("maria@example.com").Value,
            Cpf.Create("12345678909").Value,
            Phone.Create("11999998888").Value).Value;

        tutor.SoftDelete().IsSuccess.Should().BeTrue();
        var second = tutor.SoftDelete();

        second.IsSuccess.Should().BeTrue();
        tutor.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Update_ShouldReturnFailure_WhenTutorIsDeleted()
    {
        var tutor = Tutor.Create("Maria Silva",
            Email.Create("maria@example.com").Value,
            Cpf.Create("12345678909").Value,
            Phone.Create("11999998888").Value).Value;
        tutor.SoftDelete();

        var result = tutor.Update("New Name",
            Email.Create("new@example.com").Value,
            Phone.Create("11888887777").Value);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Tutor.AlreadyDeleted");
    }

    [Fact]
    public void IsActive_ShouldBeTrue_WhenNotDeleted()
    {
        var tutor = Tutor.Create("Maria Silva",
            Email.Create("maria@example.com").Value,
            Cpf.Create("12345678909").Value,
            Phone.Create("11999998888").Value).Value;

        tutor.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Update_ShouldBumpUpdatedAt_WhenSuccessful()
    {
        var tutor = Tutor.Create("Maria Silva",
            Email.Create("maria@example.com").Value,
            Cpf.Create("12345678909").Value,
            Phone.Create("11999998888").Value).Value;
        var before = tutor.UpdatedAt;
        Thread.Sleep(5);

        var result = tutor.Update("Maria Santos",
            Email.Create("maria.santos@example.com").Value,
            Phone.Create("11888887777").Value);

        result.IsSuccess.Should().BeTrue();
        tutor.UpdatedAt.Should().BeAfter(before);
    }

    [Fact]
    public void SoftDelete_ShouldBumpUpdatedAt_WhenFirstDelete()
    {
        var tutor = Tutor.Create("Maria Silva",
            Email.Create("maria@example.com").Value,
            Cpf.Create("12345678909").Value,
            Phone.Create("11999998888").Value).Value;
        var before = tutor.UpdatedAt;
        Thread.Sleep(5);

        tutor.SoftDelete().IsSuccess.Should().BeTrue();

        tutor.UpdatedAt.Should().BeAfter(before);
    }

    [Fact]
    public void Anonymize_ShouldReplaceIdentifiersWithStableTombstone()
    {
        var tutor = Tutor.Create("Maria Silva",
            Email.Create("maria@example.com").Value,
            Cpf.Create("12345678909").Value,
            Phone.Create("11999998888").Value).Value;
        var originalCpf = tutor.Cpf.Number;

        var result = tutor.Anonymize();

        result.IsSuccess.Should().BeTrue();
        tutor.IsAnonymized.Should().BeTrue();
        tutor.AnonymizedAt.Should().NotBeNull();
        tutor.Name.Should().Contain(tutor.Id.ToString("N"));
        tutor.Email.Address.Should().Contain("anonymized");
        tutor.Cpf.Number.Should().NotBe(originalCpf);
        tutor.Cpf.Number.Should().HaveLength(11);
        tutor.Phone.Number.Should().HaveLength(11);
        tutor.Address.Should().BeNull();
    }

    [Fact]
    public void Anonymize_ShouldBeIdempotent()
    {
        var tutor = Tutor.Create("Maria Silva",
            Email.Create("maria@example.com").Value,
            Cpf.Create("12345678909").Value,
            Phone.Create("11999998888").Value).Value;
        tutor.Anonymize().IsSuccess.Should().BeTrue();
        var emailAfterFirst = tutor.Email.Address;

        tutor.Anonymize().IsSuccess.Should().BeTrue();
        tutor.Email.Address.Should().Be(emailAfterFirst);
    }

    [Fact]
    public void Update_ShouldReturnFailure_WhenTutorIsAnonymized()
    {
        var tutor = Tutor.Create("Maria Silva",
            Email.Create("maria@example.com").Value,
            Cpf.Create("12345678909").Value,
            Phone.Create("11999998888").Value).Value;
        tutor.Anonymize();

        var result = tutor.Update("New Name",
            Email.Create("new@example.com").Value,
            Phone.Create("11888887777").Value);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Tutor.AlreadyAnonymized");
    }
}
