using FluentAssertions;
using Inventory.Domain.Entities;
using Inventory.Domain.Services;
using Xunit;

namespace Inventory.Tests.Domain;

public class PackageFractionationServiceTests
{
    [Fact]
    public void Plan_WhenUnitsPerPackageIsOne_ReturnsFailure()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("P", "", "SKU", "7891234567890", "UN", 0, Inventory.Domain.Enums.ProductCategory.Medication, "30049099", null, 0, null, unitsPerPackage: 1m).Value;
        var lot = ProductLot.Create(productId, "LOT-1", null, 10m, 20m).Value;

        var result = PackageFractionationService.Plan(product, lot, packagesToOpen: 1);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.InvalidUnitsPerPackage");
    }

    [Fact]
    public void Plan_WhenLotIsFractional_ReturnsFailure()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("P", "", "SKU", "7891234567890", "UN", 0, Inventory.Domain.Enums.ProductCategory.Medication, "30049099", null, 0, null, unitsPerPackage: 10m).Value;
        var lot = ProductLot.Create(productId, "LOT-1-F", null, 10m, 5m, isFractional: true).Value;

        var result = PackageFractionationService.Plan(product, lot, packagesToOpen: 1);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("StockMovement.CannotFractionateOpenLot");
    }

    [Fact]
    public void Plan_ValidRequest_ComputesQuantityAndTargetLotNumber()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("P", "", "SKU", "7891234567890", "UN", 0, Inventory.Domain.Enums.ProductCategory.Medication, "30049099", null, 0, null, unitsPerPackage: 10m, id: productId).Value;
        var lot = ProductLot.Create(productId, "LOT-1", null, 10m, 30m).Value;

        var result = PackageFractionationService.Plan(product, lot, packagesToOpen: 2);

        result.IsSuccess.Should().BeTrue();
        result.Value.QuantityToTransfer.Should().Be(20m);
        result.Value.TargetLotNumber.Should().Be("LOT-1-F");
        result.Value.SourceLotId.Should().Be(lot.Id);
    }

    [Fact]
    public void Plan_InsufficientSealedStock_ReturnsFailure()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("P", "", "SKU", "7891234567890", "UN", 0, Inventory.Domain.Enums.ProductCategory.Medication, "30049099", null, 0, null, unitsPerPackage: 10m, id: productId).Value;
        var lot = ProductLot.Create(productId, "LOT-1", null, 10m, 5m).Value;

        var result = PackageFractionationService.Plan(product, lot, packagesToOpen: 1);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ProductLot.InsufficientQuantity");
    }
}
