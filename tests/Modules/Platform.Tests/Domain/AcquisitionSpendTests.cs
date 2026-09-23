using FluentAssertions;
using Platform.Domain.Entities;

namespace Platform.Tests.Domain;

public class AcquisitionSpendTests
{
    [Fact]
    public void Create_RejectsInvalidMonth()
    {
        var result = AcquisitionSpend.Create(2026, 13, "ads", 100m, null);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_AndUpdate_PersistAmount()
    {
        var created = AcquisitionSpend.Create(2026, 3, "events", 250m, "conference").Value;
        created.Amount.Should().Be(250m);

        created.Update(300m, "updated").IsSuccess.Should().BeTrue();
        created.Amount.Should().Be(300m);
    }
}
