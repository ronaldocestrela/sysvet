using FluentAssertions;
using Veterinary.Domain.Entities;

namespace Veterinary.Tests.Domain;

public class WardUnitTests
{
    [Fact]
    public void Create_WithValidName_ReturnsSuccess()
    {
        var result = WardUnit.Create(Guid.NewGuid(), "ICU");
        result.IsSuccess.Should().BeTrue();
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ReplaceBeds_ReplacesCollection()
    {
        var unit = WardUnit.Create(Guid.NewGuid(), "Ward A").Value;
        var bedId = Guid.NewGuid();

        var replace = unit.ReplaceBeds([(bedId, "A1", 0, true)]);

        replace.IsSuccess.Should().BeTrue();
        unit.Beds.Should().HaveCount(1);
        unit.Beds.First().Code.Should().Be("A1");
    }
}
