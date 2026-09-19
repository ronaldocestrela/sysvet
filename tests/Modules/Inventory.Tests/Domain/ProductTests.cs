using FluentAssertions;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Xunit;

namespace Inventory.Tests.Domain;

public class ProductTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var name = "Ração Royal Canin 10kg";
        var barcode = "1234567890123";
        var sku = "RC-10KG";
        var unit = "Pacote";
        var reorderLevel = 5m;

        var result = Product.Create(
            name,
            "Ração seca",
            sku,
            barcode,
            unit,
            reorderLevel,
            ProductCategory.Food,
            "23091000",
            null,
            0,
            null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
        result.Value.Barcode.Should().Be(barcode);
        result.Value.Sku.Should().Be(sku);
        result.Value.RequiresLot.Should().BeFalse();
    }

    [Fact]
    public void Create_Medication_DefaultsRequiresLotTrue()
    {
        var result = Product.Create(
            "Dipirona",
            "",
            "DIP-50",
            "7891234567890",
            "UN",
            0,
            ProductCategory.Medication,
            "30049099",
            null,
            0,
            null);

        result.IsSuccess.Should().BeTrue();
        result.Value.RequiresLot.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyName_ReturnsFailure()
    {
        var result = Product.Create("", "Desc", "SKU1", "1234567890123", "UN", 0, ProductCategory.Other, "23091000", null, 0, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.InvalidName");
    }

    [Fact]
    public void Create_WithZeroUnitsPerPackage_ReturnsFailure()
    {
        var result = Product.Create(
            "Item",
            "",
            "SKU1",
            "1234567890123",
            "UN",
            0,
            ProductCategory.Other,
            "23091000",
            null,
            0,
            null,
            unitsPerPackage: 0m);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.InvalidUnitsPerPackage");
    }

    [Fact]
    public void Create_DefaultUnitsPerPackage_IsOne()
    {
        var result = Product.Create("Item", "", "SKU1", "1234567890123", "UN", 0, ProductCategory.Other, "23091000", null, 0, null);

        result.IsSuccess.Should().BeTrue();
        result.Value.UnitsPerPackage.Should().Be(1m);
    }
}
