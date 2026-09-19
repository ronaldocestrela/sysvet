using FluentAssertions;
using Inventory.Domain.Entities;
using Inventory.Domain.Services;
using Xunit;

namespace Inventory.Tests.Domain;

public class LotAllocationServiceTests
{
    [Fact]
    public void Allocate_UsesFefoOrder()
    {
        var productId = Guid.NewGuid();
        var soon = ProductLot.Create(productId, "SOON", DateTimeOffset.UtcNow.AddDays(5), 1m, 10m).Value;
        var later = ProductLot.Create(productId, "LATER", DateTimeOffset.UtcNow.AddDays(60), 1m, 10m).Value;
        var noExpiry = ProductLot.Create(productId, "NONE", null, 1m, 10m).Value;

        var allocation = LotAllocationService.Allocate(new[] { noExpiry, later, soon }, 12m);
        allocation.Should().HaveCount(2);
        allocation[0].Lot.LotNumber.Should().Be("SOON");
        allocation[0].Quantity.Should().Be(10m);
        allocation[1].Lot.LotNumber.Should().Be("LATER");
        allocation[1].Quantity.Should().Be(2m);
    }

    [Fact]
    public void Allocate_InsufficientStock_ReturnsEmpty()
    {
        var productId = Guid.NewGuid();
        var lot = ProductLot.Create(productId, "A", null, 1m, 3m).Value;
        LotAllocationService.Allocate(new[] { lot }, 5m).Should().BeEmpty();
    }

    [Fact]
    public void OrderForConsumption_PrefersFractionalLotBeforeSealed()
    {
        var productId = Guid.NewGuid();
        var sealedLot = ProductLot.Create(productId, "SEALED", DateTimeOffset.UtcNow.AddDays(1), 1m, 100m).Value;
        var fractional = ProductLot.Create(productId, "SEALED-F", DateTimeOffset.UtcNow.AddDays(1), 1m, 5m, isFractional: true).Value;

        var ordered = LotAllocationService.OrderForConsumption(new[] { sealedLot, fractional });
        ordered[0].LotNumber.Should().Be("SEALED-F");
    }
}
