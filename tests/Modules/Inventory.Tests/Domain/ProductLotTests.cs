using FluentAssertions;
using Inventory.Domain.Entities;
using Xunit;

namespace Inventory.Tests.Domain;

public class ProductLotTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = ProductLot.Create(Guid.NewGuid(), "LOT-A", DateTimeOffset.UtcNow.AddMonths(6), 12.5m, 20m);
        result.IsSuccess.Should().BeTrue();
        result.Value.LotNumber.Should().Be("LOT-A");
        result.Value.Quantity.Should().Be(20m);
    }

    [Fact]
    public void AdjustQuantity_BelowZero_ReturnsFailure()
    {
        var lot = ProductLot.Create(Guid.NewGuid(), "LOT-B", null, 1m, 5m).Value;
        var result = lot.AdjustQuantity(-10m);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ProductLot.InsufficientQuantity");
    }
}
