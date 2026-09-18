using System;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Core.Infrastructure.Identity;
using System.Net.Http.Headers;
using Inventory.Application.ProductLots.Commands;
using Inventory.Application.Products.Commands;
using Inventory.Application.Products.Dtos;
using Inventory.Application.Products.Queries;
using Inventory.Application.StockMovements.Commands;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;

namespace API.IntegrationTests.Inventory;

[Collection("IntegrationTests")]
public class ProductEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProductEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private async Task<System.Net.Http.HttpClient> CreateAuthenticatedClientAsync()
    {
        using var scope = _factory.Services.CreateScope();
        
        var coreContext = scope.ServiceProvider.GetRequiredService<Core.Infrastructure.Persistence.CoreDbContext>();
        await coreContext.Database.EnsureDeletedAsync();
        await coreContext.Database.EnsureCreatedAsync();

        var inventoryContext = scope.ServiceProvider.GetRequiredService<global::Inventory.Infrastructure.Persistence.InventoryDbContext>();
        await inventoryContext.Database.MigrateAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        var user = await userManager.FindByEmailAsync("test@sysvet.com");
        if (user == null)
        {
            user = new AppUser { UserName = "test@sysvet.com", Email = "test@sysvet.com", TenantId = Guid.NewGuid() };
            await userManager.CreateAsync(user, "Password123!");
            await userManager.AddToRoleAsync(user, "Admin");
        }

        var client = _factory.CreateClient();
        var request = new { Email = "test@sysvet.com", Password = "Password123!" };
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", request);
        
        // Use the same LoginResponse class from the same namespace/file to parse if it exists, or define it here.
        // Actually we can parse it as dynamic or we define it here inside the file just like AppointmentEndpointsTests
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse!.AccessToken);
        return client;
    }

    [Fact]
    public async Task RegisterProduct_WithValidData_ReturnsOkAndGuid()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();
        var suffix = Guid.NewGuid().ToString()[..8];
        var command = new RegisterProductCommand(
            "Test Product " + suffix,
            "A test product",
            "SKU-" + suffix,
            "7891234" + suffix,
            "KG",
            10m,
            ProductCategory.Food,
            "23091000",
            null,
            0,
            null,
            null);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/inventory/products", command);

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Status Code: {response.StatusCode}. Error: {content}");
        }
        var productId = System.Text.Json.JsonSerializer.Deserialize<Guid>(content, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        productId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RegisterStockMovement_WithValidInMovement_ReturnsOkAndGuid()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();
        var suffix = Guid.NewGuid().ToString()[..8];
        var productCommand = new RegisterProductCommand(
            "Test Product Move " + suffix,
            "To move",
            "SKU-M-" + suffix,
            "7891234" + suffix,
            "UN",
            5m,
            ProductCategory.Other,
            "23091000",
            null,
            0,
            null,
            null);
        var productResponse = await client.PostAsJsonAsync("/api/v1/inventory/products", productCommand);
        var productContent = await productResponse.Content.ReadAsStringAsync();
        if (!productResponse.IsSuccessStatusCode)
        {
            throw new Exception($"Status Code: {productResponse.StatusCode}. Error: {productContent}");
        }
        var productId = System.Text.Json.JsonSerializer.Deserialize<Guid>(productContent, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var movementCommand = new RegisterStockMovementCommand(
            productId,
            MovementType.In,
            20m,
            "LOTE001",
            DateTimeOffset.UtcNow.AddMonths(12),
            "Fornecedor Teste"
        );

        // Act
        var movementResponse = await client.PostAsJsonAsync("/api/v1/inventory/stock/movements", movementCommand);

        // Assert
        var movementContent = await movementResponse.Content.ReadAsStringAsync();
        if (!movementResponse.IsSuccessStatusCode)
        {
            throw new Exception($"Status Code: {movementResponse.StatusCode}. Error: {movementContent}");
        }
        var movementId = System.Text.Json.JsonSerializer.Deserialize<Guid>(movementContent, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        movementId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ProductWithTwoLots_ReturnsBalancePerLot()
    {
        var client = await CreateAuthenticatedClientAsync();
        var suffix = Guid.NewGuid().ToString()[..8];
        var register = new RegisterProductCommand(
            "Lot Product " + suffix,
            "",
            "SKU-L-" + suffix,
            "7895555" + suffix,
            "UN",
            0m,
            ProductCategory.Medication,
            "30049099",
            null,
            0,
            null,
            null);

        var registerResponse = await client.PostAsJsonAsync("/api/v1/inventory/products", register);
        registerResponse.EnsureSuccessStatusCode();
        var productId = await registerResponse.Content.ReadFromJsonAsync<Guid>();

        var lot1 = new RegisterProductLotCommand(productId, "LOT-A", DateTimeOffset.UtcNow.AddMonths(6), 10m, 5m);
        var lot2 = new RegisterProductLotCommand(productId, "LOT-B", DateTimeOffset.UtcNow.AddMonths(12), 12m, 3m);
        (await client.PostAsJsonAsync($"/api/v1/inventory/products/{productId}/lots", lot1)).EnsureSuccessStatusCode();
        (await client.PostAsJsonAsync($"/api/v1/inventory/products/{productId}/lots", lot2)).EnsureSuccessStatusCode();

        var detailResponse = await client.GetAsync($"/api/v1/inventory/products/{productId}");
        detailResponse.EnsureSuccessStatusCode();
        var detail = await detailResponse.Content.ReadFromJsonAsync<ProductDetailDto>();

        detail!.Lots.Should().HaveCount(2);
        detail.Lots.Single(l => l.LotNumber == "LOT-A").Quantity.Should().Be(5m);
        detail.Lots.Single(l => l.LotNumber == "LOT-B").Quantity.Should().Be(3m);
        detail.TotalQuantity.Should().Be(8m);
    }
}

public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
}
