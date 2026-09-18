using FluentAssertions;
using Inventory.Domain;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Services;
using Xunit;

namespace Inventory.Tests.Domain;

public class StockQuantityApplierTests
{
    [Theory]
    [InlineData(MovementType.In, 5, null, 5)]
    [InlineData(MovementType.Out, 5, null, -5)]
    [InlineData(MovementType.Adjustment, 3, AdjustmentDirection.Increase, 3)]
    [InlineData(MovementType.Adjustment, 3, AdjustmentDirection.Decrease, -3)]
    public void ResolveDelta_ValidCases_ReturnsExpected(
        MovementType type,
        decimal qty,
        AdjustmentDirection? direction,
        decimal expected)
    {
        var result = StockQuantityApplier.ResolveDelta(type, qty, direction);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [Fact]
    public void ResolveDelta_AdjustmentWithoutDirection_ReturnsFailure()
    {
        var result = StockQuantityApplier.ResolveDelta(MovementType.Adjustment, 1m, null);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ErrorCodes.StockMovement.InvalidAdjustmentDirection.Code);
    }

    [Fact]
    public void ValidateLotContext_RequiresLotWithoutLotId_ReturnsFailure()
    {
        var product = Product.Create("P", "D", "SKU-1", "7891234567890", "U", 0, ProductCategory.Medication, "23091000", null, 0, null).Value;
        var result = StockQuantityApplier.ValidateLotContext(product, null, null, hasAnyLots: false);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ErrorCodes.StockMovement.LotRequired.Code);
    }

    [Fact]
    public void ValidateTransferLots_SameLot_ReturnsFailure()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("P", "D", "SKU-2", "7891234567891", "U", 0, ProductCategory.Other, "23091000", null, 0, null, id: productId).Value;
        var lot = ProductLot.Create(productId, "A", null, 1m, 10m).Value;
        var result = StockQuantityApplier.ValidateTransferLots(product, lot, lot);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ErrorCodes.StockMovement.TransferSameLot.Code);
    }
}
