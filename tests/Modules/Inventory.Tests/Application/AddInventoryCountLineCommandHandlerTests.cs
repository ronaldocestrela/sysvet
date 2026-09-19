using FluentAssertions;
using Inventory.Application.InventoryCounts.Commands;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Repositories;
using NSubstitute;

namespace Inventory.Tests.Application;

public class AddInventoryCountLineCommandHandlerTests
{
    [Fact]
    public async Task Handle_ByBarcode_AddsLine()
    {
        var session = InventoryCount.Start("INV-LINE-1").Value;
        var productId = Guid.NewGuid();
        var product = Product.Create("P", "D", "SKU-L", "7891234567892", "U", 0m, ProductCategory.Medication, "23091000", null, 0, null, requiresLot: false, id: productId).Value;

        var countRepository = Substitute.For<IInventoryCountRepository>();
        countRepository.GetByIdWithLinesAsync(session.Id, Arg.Any<CancellationToken>()).Returns(session);

        var productRepository = Substitute.For<IProductRepository>();
        productRepository.GetByBarcodeAsync("7891234567892", Arg.Any<CancellationToken>()).Returns(product);

        var lotRepository = Substitute.For<IProductLotRepository>();
        lotRepository.ListByProductIdAsync(productId, Arg.Any<CancellationToken>()).Returns(Array.Empty<ProductLot>());

        var handler = new AddInventoryCountLineCommandHandler(countRepository, productRepository, lotRepository);
        var result = await handler.Handle(
            new AddInventoryCountLineCommand(session.Id, "7891234567892", null, null, 8m),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        session.Lines.Should().HaveCount(1);
        session.Lines[0].CountedQuantity.Should().Be(8m);
    }
}
