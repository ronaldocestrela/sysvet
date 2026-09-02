using System;
using FluentAssertions;
using Inventory.Domain.Entities;
using Xunit;

namespace Inventory.Tests.Domain;

public class StockMovementTests
{
    [Fact]
    public void Create_InMovement_ReturnsSuccess()
    {
        // Act
        var result = StockMovement.Create(Guid.NewGuid(), MovementType.In, 10m, "Lote01", DateTimeOffset.UtcNow.AddMonths(6), "Compra");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Quantity.Should().Be(10m);
        result.Value.Type.Should().Be(MovementType.In);
    }

    [Fact]
    public void Create_WithNegativeQuantity_ReturnsFailure()
    {
        // Act
        var result = StockMovement.Create(Guid.NewGuid(), MovementType.Out, -5m, null, null, "Venda");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("StockMovement.InvalidQuantity");
    }

    [Fact]
    public void Create_WithZeroQuantity_ReturnsFailure()
    {
        // Act
        var result = StockMovement.Create(Guid.NewGuid(), MovementType.Adjustment, 0m, null, null, "Correção");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("StockMovement.InvalidQuantity");
    }
}
