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

    [Fact]
    public void UpdateMetadata_AndSetActive_MutateLot()
    {
        var lot = ProductLot.Create(Guid.NewGuid(), "LOT-C", null, 1m, 5m).Value;
        var expiration = DateTimeOffset.UtcNow.AddMonths(3);

        lot.UpdateMetadata(expiration, 2.5m).IsSuccess.Should().BeTrue();
        lot.ExpirationDate.Should().Be(expiration);
        lot.UnitCost.Should().Be(2.5m);

        lot.SetActive(false);
        lot.IsActive.Should().BeFalse();
        lot.SetFractional(true);
        lot.IsFractional.Should().BeTrue();
        lot.SetQuantity(3m).IsSuccess.Should().BeTrue();
        lot.Quantity.Should().Be(3m);
    }

    [Fact]
    public void UpdateMetadata_WithNegativeCost_ReturnsFailure()
    {
        var lot = ProductLot.Create(Guid.NewGuid(), "LOT-D", null, 1m, 1m).Value;
        lot.UpdateMetadata(null, -1m).IsFailure.Should().BeTrue();
    }
}
