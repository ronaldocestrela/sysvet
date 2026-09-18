using Core.Domain.Entities;
using FluentAssertions;
using Veterinary.Domain.Entities;

namespace Veterinary.Tests.Domain;

public class VaccineProtocolTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = VaccineProtocol.Create(Guid.NewGuid(), "Canine core", PetSpecies.Dog);

        result.IsSuccess.Should().BeTrue();
        result.Value.Species.Should().Be(PetSpecies.Dog);
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyName_ReturnsFailure()
    {
        var result = VaccineProtocol.Create(Guid.NewGuid(), "  ", PetSpecies.Dog);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("VaccineProtocol.InvalidName");
    }

    [Fact]
    public void ReplaceDoses_WithOrderedItems_PersistsSequence()
    {
        var protocol = VaccineProtocol.Create(Guid.NewGuid(), "Core", PetSpecies.Dog).Value;
        var doseId = Guid.NewGuid();

        var replace = protocol.ReplaceDoses([
            (doseId, "1ª dose", 42, 90, null, 365)
        ]);

        replace.IsSuccess.Should().BeTrue();
        protocol.Doses.Should().HaveCount(1);
        protocol.Doses.First().Sequence.Should().Be(1);
        protocol.Doses.First().Label.Should().Be("1ª dose");
    }

    [Fact]
    public void Deactivate_WhenActive_Succeeds()
    {
        var protocol = VaccineProtocol.Create(Guid.NewGuid(), "Core", PetSpecies.Dog).Value;

        protocol.Deactivate().IsSuccess.Should().BeTrue();
        protocol.IsActive.Should().BeFalse();
    }
}
