using FluentAssertions;
using Inventory.Application.Labels;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace Inventory.Tests.Application;

public class GenerateProductLabelsQueryHandlerTests
{
    [Fact]
    public async Task Handle_Zpl_ReturnsZplContent()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("Nome", "", "SKU-X", "7891234567890", "UN", 0, ProductCategory.Other, "23091000", null, 0, null, id: productId).Value;
        var repo = Substitute.For<IProductRepository>();
        repo.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);
        var pdf = Substitute.For<IProductLabelPdfRenderer>();
        var handler = new GenerateProductLabelsQueryHandler(repo, pdf);

        var result = await handler.Handle(
            new GenerateProductLabelsQuery([new ProductLabelItemDto(productId, 1)], LabelFormat.Zpl),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var text = System.Text.Encoding.UTF8.GetString(result.Value.Content);
        text.Should().Contain("^XA");
        pdf.DidNotReceive().Render(Arg.Any<IReadOnlyList<ProductLabelRenderModel>>());
    }

    [Fact]
    public async Task Handle_Pdf_DelegatesToRenderer()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("Nome", "", "SKU-X", "7891234567890", "UN", 0, ProductCategory.Other, "23091000", null, 0, null, id: productId).Value;
        var repo = Substitute.For<IProductRepository>();
        repo.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);
        var pdf = Substitute.For<IProductLabelPdfRenderer>();
        pdf.Render(Arg.Any<IReadOnlyList<ProductLabelRenderModel>>()).Returns(new byte[] { 0x25, 0x50, 0x44, 0x46 });
        var handler = new GenerateProductLabelsQueryHandler(repo, pdf);

        var result = await handler.Handle(
            new GenerateProductLabelsQuery([new ProductLabelItemDto(productId, 2)], LabelFormat.Pdf),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ContentType.Should().Be("application/pdf");
        pdf.Received(1).Render(Arg.Any<IReadOnlyList<ProductLabelRenderModel>>());
    }
}
