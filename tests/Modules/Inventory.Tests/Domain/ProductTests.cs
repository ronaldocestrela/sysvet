using System;
using FluentAssertions;
using Inventory.Domain.Entities;
using Xunit;

namespace Inventory.Tests.Domain;

public class ProductTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var name = "Ração Royal Canin 10kg";
        var barcode = "1234567890123";
        var unit = "Pacote";
        var reorderLevel = 5m;

        // Act
        var result = Product.Create(name, "Ração seca", barcode, unit, reorderLevel);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
        result.Value.Barcode.Should().Be(barcode);
    }

    [Fact]
    public void Create_WithEmptyName_ReturnsFailure()
    {
        // Act
        var result = Product.Create("", "Desc", "123", "UN", 0);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.InvalidName");
    }
}
