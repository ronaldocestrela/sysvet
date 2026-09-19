using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Inventory.Application.Products.Commands;
using Inventory.Application.PurchaseSuggestions;
using Inventory.Application.StockMovements.Commands;
using Inventory.Application.Suppliers.Commands;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace API.IntegrationTests.Inventory;

[Collection("IntegrationTests")]
public class PurchaseSuggestionEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PurchaseSuggestionEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var coreContext = scope.ServiceProvider.GetRequiredService<Core.Infrastructure.Persistence.CoreDbContext>();
        await coreContext.Database.EnsureDeletedAsync();
        await coreContext.Database.EnsureCreatedAsync();
        var inventoryContext = scope.ServiceProvider.GetRequiredService<global::Inventory.Infrastructure.Persistence.InventoryDbContext>();
        await inventoryContext.Database.MigrateAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Core.Infrastructure.Identity.AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        var user = new Core.Infrastructure.Identity.AppUser { UserName = "test@sysvet.com", Email = "test@sysvet.com", TenantId = Guid.NewGuid() };
        await userManager.CreateAsync(user, "Password123!");
        await userManager.AddToRoleAsync(user, "Admin");

        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = "test@sysvet.com", Password = "Password123!" });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);
        return client;
    }

    [Fact]
    public async Task GetPurchaseSuggestions_WhenLowStock_ReturnsGroupedLines()
    {
        var client = await CreateAuthenticatedClientAsync();
        var supplierResponse = await client.PostAsJsonAsync("/api/v1/inventory/suppliers", new RegisterSupplierCommand("Fornecedor LTDA", "Fornecedor", "11222333000181", null, null));
        var supplierId = await supplierResponse.Content.ReadFromJsonAsync<Guid>();

        var suffix = Guid.NewGuid().ToString()[..8];
        var register = new RegisterProductCommand(
            "Low " + suffix,
            "",
            "SKU-S-" + suffix,
            "7891234" + suffix,
            "UN",
            10m,
            ProductCategory.Other,
            "23091000",
            null,
            0,
            supplierId,
            null,
            null,
            TargetStock: 30m);
        var productResponse = await client.PostAsJsonAsync("/api/v1/inventory/products", register);
        var productId = await productResponse.Content.ReadFromJsonAsync<Guid>();

        var movement = new RegisterStockMovementCommand(productId, MovementType.In, 5m, "Purchase");
        (await client.PostAsJsonAsync("/api/v1/inventory/stock/movements", movement)).EnsureSuccessStatusCode();

        var suggestions = await client.GetFromJsonAsync<IReadOnlyList<PurchaseSuggestionGroupDto>>("/api/v1/inventory/purchase-suggestions");
        suggestions.Should().NotBeNull();
        suggestions!.Should().ContainSingle(g => g.SupplierId == supplierId);
        suggestions[0].Lines.Should().ContainSingle(l => l.SuggestedQuantity == 25m);
    }

    [Fact]
    public async Task ExportPurchaseSuggestions_ReturnsCsv()
    {
        var client = await CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/api/v1/inventory/purchase-suggestions/export");
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
        var text = await response.Content.ReadAsStringAsync();
        text.Should().Contain("Fornecedor;CNPJ;Produto");
    }
}
