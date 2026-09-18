using FluentAssertions;
using Inventory.Application.StockMovements.Queries;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Repositories;
using Inventory.Domain.Services;
using NSubstitute;

namespace Inventory.Tests.Application;

public class ListStockAlertsQueryHandlerTests
{
    [Fact]
    public async Task Handle_LowStockProduct_ReturnsAlert()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("P", "D", "SKU-A", "7891234567892", "U", 10m, ProductCategory.Other, "23091000", null, 0, null, requiresLot: false, id: productId).Value;
        var balance = new ProductBalance(productId, 5m);

        var productRepository = Substitute.For<IProductRepository>();
        var lotRepository = Substitute.For<IProductLotRepository>();
        productRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { product });
        productRepository.GetBalanceAsync(productId, Arg.Any<CancellationToken>()).Returns(balance);
        lotRepository.ListByProductIdAsync(productId, Arg.Any<CancellationToken>()).Returns(Array.Empty<ProductLot>());

        var handler = new ListStockAlertsQueryHandler(productRepository, lotRepository);
        var result = await handler.Handle(new ListStockAlertsQuery(StockAlertKind.LowStock), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(a => a.ProductId == productId && a.Kind == StockAlertKind.LowStock.ToString());
    }
}
