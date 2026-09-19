using Core.Domain;
using FluentAssertions;
using Inventory.Application.StockMovements.Queries;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Repositories;
using NSubstitute;

namespace Inventory.Tests.Application;

public class StockMovementQueryHandlerTests
{
    private static Product CreateProduct() =>
        Product.Create("Ração", "", "SKU1", "1234567890123", "UN", 0, ProductCategory.Food, "23091000", null, 0, null).Value;

    [Fact]
    public async Task ListStockMovements_ProjectsDto()
    {
        var product = CreateProduct();
        var movement = StockMovement.Create(product.Id, MovementType.In, 5m, null, null, "Opening").Value;
        var movementRepo = Substitute.For<IStockMovementRepository>();
        var productRepo = Substitute.For<IProductRepository>();
        var lotRepo = Substitute.For<IProductLotRepository>();
        movementRepo.ListRecentAsync(null, null, 0, 50, Arg.Any<CancellationToken>()).Returns([movement]);
        productRepo.GetAllAsync(Arg.Any<CancellationToken>()).Returns([product]);

        var handler = new ListStockMovementsQueryHandler(movementRepo, productRepo, lotRepo);
        var result = await handler.Handle(new ListStockMovementsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(i => i.ProductId == product.Id && i.ProductName == "Ração" && i.Quantity == 5m);
    }

    [Fact]
    public async Task GetProductKardex_BuildsRunningBalance()
    {
        var product = CreateProduct();
        var inbound = StockMovement.Create(product.Id, MovementType.In, 10m, null, null, "In").Value;
        var outbound = StockMovement.Create(product.Id, MovementType.Out, 3m, null, null, "Out").Value;
        var productRepo = Substitute.For<IProductRepository>();
        var movementRepo = Substitute.For<IStockMovementRepository>();
        var lotRepo = Substitute.For<IProductLotRepository>();
        productRepo.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        movementRepo.ListByProductAsync(product.Id, Arg.Any<CancellationToken>()).Returns([inbound, outbound]);

        var handler = new GetProductKardexQueryHandler(productRepo, movementRepo, lotRepo);
        var result = await handler.Handle(new GetProductKardexQuery(product.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Last().RunningBalance.Should().Be(7m);
    }

    [Fact]
    public async Task GetProductKardex_WhenProductMissing_ReturnsNotFound()
    {
        var productRepo = Substitute.For<IProductRepository>();
        productRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);
        var handler = new GetProductKardexQueryHandler(
            productRepo,
            Substitute.For<IStockMovementRepository>(),
            Substitute.For<IProductLotRepository>());

        var result = await handler.Handle(new GetProductKardexQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.NotFound");
    }
}
