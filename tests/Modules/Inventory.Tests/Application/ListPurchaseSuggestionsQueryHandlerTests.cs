using FluentAssertions;
using Inventory.Application.PurchaseSuggestions;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace Inventory.Tests.Application;

public class ListPurchaseSuggestionsQueryHandlerTests
{
    [Fact]
    public async Task Handle_GroupsBySupplier_WithSuggestedQuantity()
    {
        var supplierId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = Product.Create(
            "Ração",
            "",
            "RAC-1",
            "7891234567890",
            "UN",
            reorderLevel: 10m,
            ProductCategory.Food,
            "23091000",
            null,
            0,
            supplierId,
            targetStock: 30m,
            id: productId).Value;

        var productRepo = Substitute.For<IProductRepository>();
        productRepo.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { product });
        productRepo.GetBalanceAsync(productId, Arg.Any<CancellationToken>()).Returns(new ProductBalance(productId, 5m));

        var lotRepo = Substitute.For<IProductLotRepository>();
        lotRepo.ListByProductIdAsync(productId, Arg.Any<CancellationToken>()).Returns(Array.Empty<ProductLot>());

        var supplier = Supplier.Create("Fornecedor LTDA", "Fornecedor", "11222333000181", null, null, supplierId).Value;
        var supplierRepo = Substitute.For<ISupplierRepository>();
        supplierRepo.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { supplier });

        var handler = new ListPurchaseSuggestionsQueryHandler(productRepo, lotRepo, supplierRepo);
        var result = await handler.Handle(new ListPurchaseSuggestionsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Lines.Should().ContainSingle();
        result.Value[0].Lines[0].SuggestedQuantity.Should().Be(25m);
    }
}
