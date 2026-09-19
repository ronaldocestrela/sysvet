using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Inventory.Application.Products.Commands;
using Inventory.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace API.IntegrationTests.Inventory;

[Collection("IntegrationTests")]
public class ProductLabelEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProductLabelEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

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
    public async Task GetProductLabel_Pdf_ReturnsPdfBytes()
    {
        var client = await CreateAuthenticatedClientAsync();
        var suffix = Guid.NewGuid().ToString()[..8];
        var register = new RegisterProductCommand(
            "Label Product " + suffix,
            "",
            "SKU-L-" + suffix,
            "7891234" + suffix,
            "UN",
            0m,
            ProductCategory.Other,
            "23091000",
            null,
            0,
            null,
            null);
        var productId = await client.PostAsJsonAsync("/api/v1/inventory/products", register);
        productId.EnsureSuccessStatusCode();
        var id = await productId.Content.ReadFromJsonAsync<Guid>();

        var response = await client.GetAsync($"/api/v1/inventory/products/{id}/label?format=pdf&copies=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Length.Should().BeGreaterThan(4);
        bytes[0].Should().Be(0x25);
        bytes[1].Should().Be((byte)'P');
    }

    [Fact]
    public async Task GetProductLabel_Zpl_ReturnsZplContent()
    {
        var client = await CreateAuthenticatedClientAsync();
        var suffix = Guid.NewGuid().ToString()[..8];
        var register = new RegisterProductCommand(
            "ZPL " + suffix,
            "",
            "SKU-Z-" + suffix,
            "7891234" + suffix,
            "UN",
            0m,
            ProductCategory.Other,
            "23091000",
            null,
            0,
            null,
            null);
        var productId = await client.PostAsJsonAsync("/api/v1/inventory/products", register);
        var id = await productId.Content.ReadFromJsonAsync<Guid>();

        var response = await client.GetAsync($"/api/v1/inventory/products/{id}/label?format=zpl&copies=1");
        var text = await response.Content.ReadAsStringAsync();
        text.Should().Contain("^XA");
        text.Should().Contain("^BC");
    }
}
