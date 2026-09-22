using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Commerce.Application.Dtos;
using Commerce.Application.Marketplace;
using Commerce.Infrastructure.Marketplace;
using Core.Application.Authorization;
using Core.Infrastructure.Identity;
using Core.Infrastructure.Persistence;
using FluentAssertions;
using Inventory.Application.Products.Commands;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace API.IntegrationTests.Commerce;

[Collection("IntegrationTests")]
public class CommerceEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Password = "Password123!";

    private readonly WebApplicationFactory<Program> _factory;

    public CommerceEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task EcommerceOrder_DebitsStock_WhenConfirmed()
    {
        var staff = await CreateStaffClientAsync();
        var (productId, offerId) = await SeedProductOfferAsync(staff, stockQty: 5m, price: 19.9m);
        const string slug = "loja-pet-test";

        await PublishClinicSiteAsync(staff, slug);

        var publicClient = _factory.CreateClient();
        var orderResponse = await publicClient.PostAsJsonAsync(
            $"/api/v1/public/clinic-sites/{slug}/store/orders",
            new
            {
                buyerName = "Cliente Loja",
                buyerPhone = "11977776666",
                buyerEmail = "cliente@test.com",
                lines = new[] { new { OfferId = offerId, Quantity = 2m } }
            });
        var orderBody = await orderResponse.Content.ReadAsStringAsync();
        orderResponse.StatusCode.Should().Be(HttpStatusCode.OK, orderBody);

        using var scope = _factory.Services.CreateScope();
        var inventory = scope.ServiceProvider.GetRequiredService<global::Inventory.Infrastructure.Persistence.InventoryDbContext>();
        var movements = await inventory.StockMovements
            .Where(m => m.Reason.Contains("Sale"))
            .ToListAsync();
        movements.Should().NotBeEmpty();
        movements.Where(m => m.ProductId == productId).Sum(m => m.Quantity).Should().Be(2m);
    }

    [Fact]
    public async Task OfferPrice_ReflectsBackofficeChange_OnPublicCatalog()
    {
        var staff = await CreateStaffClientAsync();
        var (_, offerId) = await SeedProductOfferAsync(staff, stockQty: 3m, price: 10m);
        const string slug = "preco-loja";

        await PublishClinicSiteAsync(staff, slug);

        var update = await staff.PutAsJsonAsync("/api/v1/commerce/offers", new
        {
            ProductId = await GetOfferProductIdAsync(staff, offerId),
            SalePrice = 42.5m,
            IsPublished = true,
            StoreEnabled = true,
            MercadoLivreEnabled = false
        });
        update.EnsureSuccessStatusCode();

        var publicClient = _factory.CreateClient();
        var catalog = await publicClient.GetFromJsonAsync<List<PublicStoreProductDto>>(
            $"/api/v1/public/clinic-sites/{slug}/store/catalog");
        catalog.Should().NotBeNull();
        catalog!.First(p => p.OfferId == offerId).SalePrice.Should().Be(42.5m);
    }

    [Fact]
    public async Task MercadoLivreInboundOrder_DebitsStock()
    {
        var staff = await CreateStaffClientAsync();
        var (productId, offerId) = await SeedProductOfferAsync(staff, stockQty: 4m, price: 15m, sku: "ML-SKU-1");

        var offers = await staff.GetFromJsonAsync<List<ProductOfferDto>>("/api/v1/commerce/offers");
        var offer = offers!.First(o => o.Id == offerId);

        await staff.PutAsJsonAsync("/api/v1/commerce/marketplace/mercadolivre", new
        {
            accessToken = "test-token",
            userId = 999888777L,
            siteId = "MLB",
            isEnabled = true
        });

        using (var scope = _factory.Services.CreateScope())
        {
            var fake = scope.ServiceProvider.GetService<FakeMarketplaceChannel>();
            fake.Should().NotBeNull();
            fake!.SeedOrder(new MarketplaceRemoteOrder(
                "ML-ORDER-1",
                "Comprador ML",
                "11966665555",
                null,
                [new MarketplaceRemoteOrderLine("ML-SKU-1", 1m, 15m)]));
        }

        var publicClient = _factory.CreateClient();
        var webhook = await publicClient.PostAsJsonAsync(
            "/api/v1/public/commerce/marketplaces/mercadolivre/notifications?user_id=999888777",
            new { topic = "orders_v2", resource = "/orders/ML-ORDER-1" });
        webhook.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        using var verifyScope = _factory.Services.CreateScope();
        var inventory = verifyScope.ServiceProvider.GetRequiredService<global::Inventory.Infrastructure.Persistence.InventoryDbContext>();
        var qty = await inventory.StockMovements
            .Where(m => m.ProductId == productId && m.Reason.Contains("Sale"))
            .SumAsync(m => m.Quantity);
        qty.Should().BeGreaterThanOrEqualTo(1m);
    }

    private async Task<(Guid ProductId, Guid OfferId)> SeedProductOfferAsync(
        HttpClient staff,
        decimal stockQty,
        decimal price,
        string? sku = null)
    {
        var suffix = Guid.NewGuid().ToString()[..8];
        var productSku = sku ?? "EC-" + suffix;

        var productResponse = await staff.PostAsJsonAsync("/api/v1/inventory/products", new RegisterProductCommand(
            "Produto Loja " + suffix,
            "",
            productSku,
            "880" + suffix,
            "UN",
            0m,
            ProductCategory.Food,
            "23091000",
            null,
            0,
            null,
            null));
        productResponse.EnsureSuccessStatusCode();
        var productId = await productResponse.Content.ReadFromJsonAsync<Guid>();

        await staff.PostAsJsonAsync("/api/v1/inventory/stock/movements", new
        {
            productId,
            type = MovementType.In,
            quantity = stockQty,
            reason = "Opening"
        });

        var offerResponse = await staff.PutAsJsonAsync("/api/v1/commerce/offers", new
        {
            ProductId = productId,
            SalePrice = price,
            IsPublished = true,
            StoreEnabled = true,
            MercadoLivreEnabled = true
        });
        offerResponse.EnsureSuccessStatusCode();
        var offer = await offerResponse.Content.ReadFromJsonAsync<ProductOfferDto>();

        return (productId, offer!.Id);
    }

    private static async Task PublishClinicSiteAsync(HttpClient staff, string slug)
    {
        var put = await staff.PutAsJsonAsync("/api/v1/clinic-site", new
        {
            DisplayName = "Clínica Loja",
            Tagline = (string?)null,
            Street = "Rua Loja",
            Number = "10",
            Complement = (string?)null,
            District = "Centro",
            City = "São Paulo",
            State = "SP",
            PostalCode = "01001000",
            Phone = "11988887777",
            Email = "loja@test.com",
            WhatsApp = (string?)null,
            LogoUrl = (string?)null,
            Slug = slug
        });
        put.EnsureSuccessStatusCode();

        var publish = await staff.PostAsync("/api/v1/clinic-site/publish", null);
        publish.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    private static async Task<Guid> GetOfferProductIdAsync(HttpClient staff, Guid offerId)
    {
        var offers = await staff.GetFromJsonAsync<List<ProductOfferDto>>("/api/v1/commerce/offers");
        return offers!.First(o => o.Id == offerId).ProductId;
    }

    private async Task<HttpClient> CreateStaffClientAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        await PrepareDatabasesAsync(scope);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in ApplicationRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var email = $"commerce-{Guid.NewGuid():N}@sysvet.com";
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            TenantId = IntegrationTestDatabaseHelper.SingleTenantId
        };
        await userManager.CreateAsync(user, Password);
        await userManager.AddToRoleAsync(user, ApplicationRoles.Admin);

        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password });
        var token = await login.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);
        return client;
    }

    private static async Task PrepareDatabasesAsync(IServiceScope scope)
    {
        var coreContext = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
        await coreContext.Database.EnsureDeletedAsync();
        await coreContext.Database.MigrateAsync();
        await IntegrationTestDatabaseHelper.MigrateModuleDatabasesAsync(scope);

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in ApplicationRoles.AllIncludingTutor)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private sealed record LoginResponse(string AccessToken, string RefreshToken);
}
