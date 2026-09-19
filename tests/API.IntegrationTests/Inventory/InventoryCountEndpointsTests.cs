using System.Net.Http.Json;
using FluentAssertions;
using Inventory.Application.InventoryCounts.Dtos;
using Inventory.Application.ProductLots.Commands;
using Inventory.Application.Products.Commands;
using Inventory.Application.Products.Dtos;
using Inventory.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace API.IntegrationTests.Inventory;

[Collection("IntegrationTests")]
public class InventoryCountEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public InventoryCountEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task BlindCount_SubmitApprove_UpdatesProductBalance()
    {
        var client = await ProductEndpointsTestsHelper.CreateAuthenticatedClientAsync(_factory);
        var suffix = Guid.NewGuid().ToString()[..8];
        var barcode = "7895555" + suffix;

        var register = new RegisterProductCommand(
            "Inv Product " + suffix,
            "",
            "SKU-INV-" + suffix,
            barcode,
            "UN",
            0m,
            ProductCategory.Medication,
            "30049099",
            null,
            0,
            null,
            false);

        var registerResponse = await client.PostAsJsonAsync("/api/v1/inventory/products", register);
        registerResponse.EnsureSuccessStatusCode();
        var productId = await registerResponse.Content.ReadFromJsonAsync<Guid>();

        var byBarcode = await client.GetAsync($"/api/v1/inventory/products/by-barcode/{barcode}");
        byBarcode.EnsureSuccessStatusCode();

        var startResponse = await client.PostAsync("/api/v1/inventory/counts", null);
        startResponse.EnsureSuccessStatusCode();
        var sessionId = await startResponse.Content.ReadFromJsonAsync<Guid>();

        var blindDetail = await client.GetFromJsonAsync<InventoryCountDetailDto>($"/api/v1/inventory/counts/{sessionId}");
        blindDetail!.Status.Should().Be(InventoryCountStatus.InProgress);

        var lineBody = new { Barcode = barcode, ProductId = (Guid?)null, ProductLotId = (Guid?)null, QuantityToAdd = 8m };
        var lineResponse = await client.PostAsJsonAsync($"/api/v1/inventory/counts/{sessionId}/lines", lineBody);
        lineResponse.EnsureSuccessStatusCode();

        var inProgressDetail = await client.GetFromJsonAsync<InventoryCountDetailDto>($"/api/v1/inventory/counts/{sessionId}");
        inProgressDetail!.Lines.Should().HaveCount(1);
        inProgressDetail.Lines[0].ExpectedQuantity.Should().BeNull();
        inProgressDetail.Lines[0].Variance.Should().BeNull();

        var submitResponse = await client.PostAsync($"/api/v1/inventory/counts/{sessionId}/submit", null);
        submitResponse.EnsureSuccessStatusCode();

        var submittedDetail = await client.GetFromJsonAsync<InventoryCountDetailDto>($"/api/v1/inventory/counts/{sessionId}");
        submittedDetail!.Status.Should().Be(InventoryCountStatus.Submitted);
        submittedDetail.Lines[0].ExpectedQuantity.Should().Be(0m);
        submittedDetail.Lines[0].Variance.Should().Be(8m);

        var approveResponse = await client.PostAsync($"/api/v1/inventory/counts/{sessionId}/approve", null);
        approveResponse.EnsureSuccessStatusCode();

        var productDetail = await client.GetFromJsonAsync<ProductDetailDto>($"/api/v1/inventory/products/{productId}");
        productDetail!.TotalQuantity.Should().Be(8m);
    }

    [Fact]
    public async Task LotProduct_RequiresLotOnLine()
    {
        var client = await ProductEndpointsTestsHelper.CreateAuthenticatedClientAsync(_factory);
        var suffix = Guid.NewGuid().ToString()[..8];
        var barcode = "7894444" + suffix;

        var register = new RegisterProductCommand(
            "Lot Inv " + suffix,
            "",
            "SKU-LINV-" + suffix,
            barcode,
            "UN",
            0m,
            ProductCategory.Medication,
            "30049099",
            null,
            0,
            null,
            true);

        var registerResponse = await client.PostAsJsonAsync("/api/v1/inventory/products", register);
        registerResponse.EnsureSuccessStatusCode();
        var productId = await registerResponse.Content.ReadFromJsonAsync<Guid>();

        var lot = new RegisterProductLotCommand(productId, "LOT-INV", DateTimeOffset.UtcNow.AddMonths(6), 5m, 10m);
        var lotResponse = await client.PostAsJsonAsync($"/api/v1/inventory/products/{productId}/lots", lot);
        lotResponse.EnsureSuccessStatusCode();
        var lotId = await lotResponse.Content.ReadFromJsonAsync<Guid>();

        var startResponse = await client.PostAsync("/api/v1/inventory/counts", null);
        var sessionId = await startResponse.Content.ReadFromJsonAsync<Guid>();

        var withoutLot = await client.PostAsJsonAsync(
            $"/api/v1/inventory/counts/{sessionId}/lines",
            new { Barcode = barcode, ProductId = (Guid?)null, ProductLotId = (Guid?)null, QuantityToAdd = 1m });
        withoutLot.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        var withLot = await client.PostAsJsonAsync(
            $"/api/v1/inventory/counts/{sessionId}/lines",
            new { Barcode = (string?)null, ProductId = productId, ProductLotId = lotId, QuantityToAdd = 9m });
        withLot.EnsureSuccessStatusCode();
    }
}
