using FluentAssertions;
using Inventory.Domain;
using Xunit;

namespace Inventory.Tests.Domain;

public class StockLossReasonsTests
{
    [Theory]
    [InlineData(StockLossReasons.Expired, StockMovementReasons.LossExpired)]
    [InlineData(StockLossReasons.Damage, StockMovementReasons.LossDamage)]
    [InlineData(StockLossReasons.InternalConsumption, StockMovementReasons.InternalConsumption)]
    [InlineData(StockLossReasons.Donation, StockMovementReasons.Donation)]
    public void ToMovementReason_KnownCodes_MapCorrectly(string lossReason, string expectedMovementReason)
    {
        StockLossReasons.ToMovementReason(lossReason).Should().Be(expectedMovementReason);
    }

    [Fact]
    public void IsValid_UnknownCode_ReturnsFalse()
    {
        StockLossReasons.IsValid("Unknown").Should().BeFalse();
    }

    [Fact]
    public void IsValid_KnownCode_ReturnsTrue()
    {
        StockLossReasons.IsValid(StockLossReasons.Expired).Should().BeTrue();
    }
}
