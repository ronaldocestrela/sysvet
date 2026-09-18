using System.Net.Http.Json;
using FluentAssertions;
using Inventory.Application.ProductLots.Commands;
using Inventory.Application.Products.Commands;
using Inventory.Application.Products.Dtos;
using Inventory.Application.StockMovements.Commands;
using Inventory.Application.StockMovements.Dtos;
using Inventory.Application.StockMovements.Queries;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace API.IntegrationTests.Inventory;

[Collection("IntegrationTests")]
public class StockMovementEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public StockMovementEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task OutMovement_OnLot_ReducesLotBalance()
    {
        var client = await ProductEndpointsTestsHelper.CreateAuthenticatedClientAsync(_factory);
        var suffix = Guid.NewGuid().ToString()[..8];
        var register = new RegisterProductCommand(
            "Stock Out " + suffix,
            "",
            "SKU-OUT-" + suffix,
            "7896666" + suffix,
            "UN",
            10m,
            ProductCategory.Medication,
            "30049099",
            null,
            0,
            null,
            null);

        var registerResponse = await client.PostAsJsonAsync("/api/v1/inventory/products", register);
        registerResponse.EnsureSuccessStatusCode();
        var productId = await registerResponse.Content.ReadFromJsonAsync<Guid>();

        var lot = new RegisterProductLotCommand(productId, "LOT-OUT", DateTimeOffset.UtcNow.AddMonths(6), 5m, 20m);
        (await client.PostAsJsonAsync($"/api/v1/inventory/products/{productId}/lots", lot)).EnsureSuccessStatusCode();

        var detailBefore = await (await client.GetAsync($"/api/v1/inventory/products/{productId}")).Content.ReadFromJsonAsync<ProductDetailDto>();
        var lotId = detailBefore!.Lots.Single().Id;

        var movement = new RegisterStockMovementCommand(
            productId,
            MovementType.Out,
            4m,
            "Manual out",
            lotId);

        var movementResponse = await client.PostAsJsonAsync("/api/v1/inventory/stock/movements", movement);
        movementResponse.EnsureSuccessStatusCode();

        var detailAfter = await (await client.GetAsync($"/api/v1/inventory/products/{productId}")).Content.ReadFromJsonAsync<ProductDetailDto>();
        detailAfter!.Lots.Single().Quantity.Should().Be(16m);
        detailAfter.TotalQuantity.Should().Be(16m);
    }

    [Fact]
    public async Task LowStockProduct_AppearsInStockAlerts()
    {
        var client = await ProductEndpointsTestsHelper.CreateAuthenticatedClientAsync(_factory);
        var suffix = Guid.NewGuid().ToString()[..8];
        var register = new RegisterProductCommand(
            "Low Stock " + suffix,
            "",
            "SKU-LOW-" + suffix,
            "7897777" + suffix,
            "UN",
            15m,
            ProductCategory.Other,
            "23091000",
            null,
            0,
            null,
            false);

        var registerResponse = await client.PostAsJsonAsync("/api/v1/inventory/products", register);
        registerResponse.EnsureSuccessStatusCode();
        var productId = await registerResponse.Content.ReadFromJsonAsync<Guid>();

        (await client.PostAsJsonAsync("/api/v1/inventory/stock/movements",
            new RegisterStockMovementCommand(productId, MovementType.In, 10m, "Initial"))).EnsureSuccessStatusCode();

        var detail = await (await client.GetAsync($"/api/v1/inventory/products/{productId}")).Content.ReadFromJsonAsync<ProductDetailDto>();
        detail!.TotalQuantity.Should().Be(10m);
        detail.TotalQuantity.Should().BeLessThanOrEqualTo(detail.ReorderLevel);
    }
}

internal static class ProductEndpointsTestsHelper
{
    public static async Task<HttpClient> CreateAuthenticatedClientAsync(WebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var coreContext = scope.ServiceProvider.GetRequiredService<Core.Infrastructure.Persistence.CoreDbContext>();
        await coreContext.Database.EnsureDeletedAsync();
        await coreContext.Database.EnsureCreatedAsync();
        var inventoryContext = scope.ServiceProvider.GetRequiredService<global::Inventory.Infrastructure.Persistence.InventoryDbContext>();
        await inventoryContext.Database.MigrateAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Core.Infrastructure.Identity.AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole("Admin"));
        }

        var user = await userManager.FindByEmailAsync("test@sysvet.com");
        if (user is null)
        {
            user = new Core.Infrastructure.Identity.AppUser { UserName = "test@sysvet.com", Email = "test@sysvet.com", TenantId = Guid.NewGuid() };
            await userManager.CreateAsync(user, "Password123!");
            await userManager.AddToRoleAsync(user, "Admin");
        }

        var client = factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = "test@sysvet.com", Password = "Password123!" });
        var token = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token!.AccessToken);
        return client;
    }
}
