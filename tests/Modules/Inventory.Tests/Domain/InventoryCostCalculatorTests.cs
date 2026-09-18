using FluentAssertions;
using Inventory.Domain.Services;
using Xunit;

namespace Inventory.Tests.Domain;

public class InventoryCostCalculatorTests
{
    [Fact]
    public void WeightedAverageCost_WithTwoLots_ReturnsWeightedValue()
    {
        var avg = InventoryCostCalculator.WeightedAverageCost([
            (10m, 5m),
            (5m, 8m)
        ]);

        avg.Should().Be(6m);
    }

    [Fact]
    public void TotalQuantityFromLots_ExcludesInactive()
    {
        var total = InventoryCostCalculator.TotalQuantityFromLots([
            (10m, true),
            (5m, false),
            (3m, true)
        ]);

        total.Should().Be(13m);
    }
}
