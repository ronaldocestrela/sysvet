using Core.Domain;
using Core.Domain.Auditing;
using FluentAssertions;
using Inventory.Application.ProductLots.Commands;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Repositories;
using NSubstitute;

namespace Inventory.Tests.Application;

public class ProductLotCommandHandlerTests
{
    private static Product CreateProduct() =>
        Product.Create("Item", "", "SKU1", "1234567890123", "UN", 0, ProductCategory.Other, "23091000", null, 0, null).Value;

    [Fact]
    public async Task Update_WhenLotExists_UpdatesMetadata()
    {
        var product = CreateProduct();
        var lot = ProductLot.Create(product.Id, "LOT-1", null, 1m, 5m).Value;
        var productRepo = Substitute.For<IProductRepository>();
        var lotRepo = Substitute.For<IProductLotRepository>();
        var tenant = Substitute.For<ITenantContext>();
        var audit = Substitute.For<IAuditLogger>();
        tenant.TenantId.Returns(Guid.NewGuid());
        tenant.UserId.Returns(Guid.NewGuid());
        lotRepo.GetByIdAsync(lot.Id, Arg.Any<CancellationToken>()).Returns(lot);
        productRepo.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        productRepo.GetBalanceAsync(product.Id, Arg.Any<CancellationToken>()).Returns(new ProductBalance(product.Id, 5m));
        lotRepo.ListByProductIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns([lot]);

        var handler = new UpdateProductLotCommandHandler(productRepo, lotRepo, tenant, audit);
        var result = await handler.Handle(new UpdateProductLotCommand(lot.Id, DateTimeOffset.UtcNow.AddMonths(1), 2m), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        lot.UnitCost.Should().Be(2m);
        lotRepo.Received(1).Update(lot);
    }

    [Fact]
    public async Task Update_WhenLotMissing_ReturnsNotFound()
    {
        var lotRepo = Substitute.For<IProductLotRepository>();
        lotRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ProductLot?)null);
        var handler = new UpdateProductLotCommandHandler(
            Substitute.For<IProductRepository>(),
            lotRepo,
            Substitute.For<ITenantContext>(),
            Substitute.For<IAuditLogger>());

        var result = await handler.Handle(new UpdateProductLotCommand(Guid.NewGuid(), null, 1m), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ProductLot.NotFound");
    }

    [Fact]
    public async Task SetActive_DeactivatesLot()
    {
        var product = CreateProduct();
        var lot = ProductLot.Create(product.Id, "LOT-2", null, 1m, 5m).Value;
        var productRepo = Substitute.For<IProductRepository>();
        var lotRepo = Substitute.For<IProductLotRepository>();
        var tenant = Substitute.For<ITenantContext>();
        var audit = Substitute.For<IAuditLogger>();
        tenant.TenantId.Returns(Guid.NewGuid());
        tenant.UserId.Returns(Guid.NewGuid());
        lotRepo.GetByIdAsync(lot.Id, Arg.Any<CancellationToken>()).Returns(lot);
        productRepo.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        productRepo.GetBalanceAsync(product.Id, Arg.Any<CancellationToken>()).Returns(new ProductBalance(product.Id, 5m));
        lotRepo.ListByProductIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns([lot]);

        var handler = new SetProductLotActiveCommandHandler(productRepo, lotRepo, tenant, audit);
        var result = await handler.Handle(new SetProductLotActiveCommand(lot.Id, false), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        lot.IsActive.Should().BeFalse();
    }
}
