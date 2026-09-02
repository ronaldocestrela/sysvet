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
using Inventory.Application.Products.Commands;
using Inventory.Application.StockMovements.Commands;
using Inventory.Domain.Entities;

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
        var command = new RegisterProductCommand(
            "Test Product " + Guid.NewGuid().ToString().Substring(0, 8),
            "A test product",
            "BARCODE-" + Guid.NewGuid().ToString().Substring(0, 8),
            "KG",
            10m
        );

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
        var productCommand = new RegisterProductCommand(
            "Test Product Move " + Guid.NewGuid().ToString().Substring(0, 8),
            "To move",
            "BARCODE-MOVE-" + Guid.NewGuid().ToString().Substring(0, 8),
            "UN",
            5m
        );
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
}

public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
}
