using Core.Domain;
using Core.Domain.Auditing;
using FluentAssertions;
using Inventory.Application.Products.Commands;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Repositories;
using NSubstitute;

namespace Inventory.Tests.Application;

public class UpdateProductCommandHandlerTests
{
    private static Product CreateProduct(string sku = "SKU-1", string barcode = "1234567890123") =>
        Product.Create("Ração", "", sku, barcode, "UN", 0, ProductCategory.Food, "23091000", null, 0, null).Value;

    [Fact]
    public async Task Handle_WithValidCommand_UpdatesProduct()
    {
        var product = CreateProduct();
        var productRepo = Substitute.For<IProductRepository>();
        var supplierRepo = Substitute.For<ISupplierRepository>();
        var tenant = Substitute.For<ITenantContext>();
        var audit = Substitute.For<IAuditLogger>();
        tenant.TenantId.Returns(Guid.NewGuid());
        tenant.UserId.Returns(Guid.NewGuid());
        productRepo.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        productRepo.GetBySkuAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Product?)null);
        productRepo.GetByBarcodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Product?)null);

        var handler = new UpdateProductCommandHandler(productRepo, supplierRepo, tenant, audit);
        var result = await handler.Handle(new UpdateProductCommand(
            product.Id,
            "Ração Plus",
            "desc",
            "SKU-2",
            "1234567890999",
            "KG",
            2m,
            ProductCategory.Food,
            "23091000",
            null,
            0,
            null,
            false), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        productRepo.Received(1).Update(product);
        product.Name.Should().Be("Ração Plus");
    }

    [Fact]
    public async Task Handle_WhenProductMissing_ReturnsNotFound()
    {
        var productRepo = Substitute.For<IProductRepository>();
        productRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);
        var handler = new UpdateProductCommandHandler(
            productRepo,
            Substitute.For<ISupplierRepository>(),
            Substitute.For<ITenantContext>(),
            Substitute.For<IAuditLogger>());

        var result = await handler.Handle(new UpdateProductCommand(
            Guid.NewGuid(), "X", "", "SKU", "1234567890123", "UN", 0, ProductCategory.Other, "23091000", null, 0, null, false),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.NotFound");
    }

    [Fact]
    public async Task Handle_WhenSkuBelongsToAnotherProduct_ReturnsConflict()
    {
        var product = CreateProduct();
        var other = CreateProduct("SKU-OTHER", "9999567890123");
        var productRepo = Substitute.For<IProductRepository>();
        productRepo.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        productRepo.GetBySkuAsync("SKU-OTHER", Arg.Any<CancellationToken>()).Returns(other);

        var handler = new UpdateProductCommandHandler(
            productRepo,
            Substitute.For<ISupplierRepository>(),
            Substitute.For<ITenantContext>(),
            Substitute.For<IAuditLogger>());

        var result = await handler.Handle(new UpdateProductCommand(
            product.Id, "X", "", "SKU-OTHER", "1234567890123", "UN", 0, ProductCategory.Other, "23091000", null, 0, null, false),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.SkuConflict");
    }
}

public class SetProductActiveCommandHandlerTests
{
    [Fact]
    public async Task Handle_DeactivatesExistingProduct()
    {
        var product = Product.Create("Item", "", "SKU1", "1234567890123", "UN", 0, ProductCategory.Other, "23091000", null, 0, null).Value;
        var productRepo = Substitute.For<IProductRepository>();
        var tenant = Substitute.For<ITenantContext>();
        var audit = Substitute.For<IAuditLogger>();
        tenant.TenantId.Returns(Guid.NewGuid());
        tenant.UserId.Returns(Guid.NewGuid());
        productRepo.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        var handler = new SetProductActiveCommandHandler(productRepo, tenant, audit);
        var result = await handler.Handle(new SetProductActiveCommand(product.Id, false), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        product.IsActive.Should().BeFalse();
        productRepo.Received(1).Update(product);
    }

    [Fact]
    public async Task Handle_WhenMissing_ReturnsNotFound()
    {
        var productRepo = Substitute.For<IProductRepository>();
        productRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);

        var handler = new SetProductActiveCommandHandler(
            productRepo,
            Substitute.For<ITenantContext>(),
            Substitute.For<IAuditLogger>());

        var result = await handler.Handle(new SetProductActiveCommand(Guid.NewGuid(), true), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.NotFound");
    }
}
