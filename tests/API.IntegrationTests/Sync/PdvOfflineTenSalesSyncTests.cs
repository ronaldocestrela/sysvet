using System.Net.Http.Headers;
using System.Net.Http.Json;
using Clients.Infrastructure.Http;
using Clients.Infrastructure.Sales;
using Clients.Infrastructure.Sync;
using FluentAssertions;
using Inventory.Application.Products.Commands;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sales.Domain.Enums;
using Xunit;

namespace API.IntegrationTests.Sync;

[Collection("IntegrationTests")]
public class PdvOfflineTenSalesSyncTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PdvOfflineTenSalesSyncTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task TenOfflineSales_SyncTwice_DoesNotDuplicateOrders()
    {
        var apiClient = await CreateAuthenticatedClientAsync();
        await using var harness = await SyncPocHarness.CreateAsync(apiClient);

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var productResponse = await apiClient.PostAsJsonAsync("/api/v1/inventory/products", new RegisterProductCommand(
            "Offline PDV " + suffix,
            "",
            "OPDV-" + suffix,
            "999" + suffix,
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

        var stockIn = await apiClient.PostAsJsonAsync("/api/v1/inventory/stock/movements", new
        {
            productId,
            type = MovementType.In,
            quantity = 100m,
            reason = "Seed"
        });
        stockIn.EnsureSuccessStatusCode();

        var localProduct = Product.Create("Offline PDV", "d", "OPDV-" + suffix, "999" + suffix, "UN", 0, ProductCategory.Food, "23091000", null, 0, null, id: productId);
        localProduct.IsSuccess.Should().BeTrue();
        harness.OfflineDb.Products.Add(localProduct.Value);
        harness.OfflineDb.ProductBalances.Add(new ProductBalance(productId, 100m));
        await harness.OfflineDb.SaveChangesAsync();

        var wake = new SyncWakeSignal();
        var connectivity = new FakeSyncConnectivity();
        connectivity.SetOnline(true);
        var salesStore = new OfflineSalesStore(
            harness.OfflineDb,
            wake,
            connectivity,
            new global::Sales.Domain.Payments.SimulatedPaymentTerminal(),
            new Clients.Infrastructure.Fiscal.OfflineFiscalNfceService(harness.OfflineDb));

        var register = await salesStore.OpenCashRegisterAsync(50m);
        register.IsSuccess.Should().BeTrue();

        for (var i = 0; i < 10; i++)
        {
            var sale = await salesStore.CreateAndPayOrderAsync(new CreateSalesOrderClientRequest
            {
                CashRegisterId = register.Value,
                Items =
                [
                    new SalesOrderItemClientDto
                    {
                        Kind = "Product",
                        ProductId = productId,
                        ProductName = "Offline PDV",
                        Quantity = 1m,
                        UnitPrice = 10m
                    },
                    new SalesOrderItemClientDto
                    {
                        Kind = "Service",
                        ProductName = "Consulta",
                        Quantity = 1m,
                        UnitPrice = 5m
                    }
                ]
            },
            [
                new PayOrderPaymentClientDto { Method = "Cash", Amount = 10m },
                new PayOrderPaymentClientDto { Method = "Pix", Amount = 5m }
            ]);
            sale.IsSuccess.Should().BeTrue();
        }

        var first = await harness.RunSyncCycleAsync();
        first.PendingCount.Should().Be(0);
        first.ErrorCount.Should().Be(0);

        using (var scope = _factory.Services.CreateScope())
        {
            var salesDb = scope.ServiceProvider.GetRequiredService<global::Sales.Infrastructure.Persistence.SalesDbContext>();
            var paid = await salesDb.Orders.IgnoreQueryFilters().CountAsync(o => o.Status == OrderStatus.Paid);
            paid.Should().Be(10);
        }

        var second = await harness.RunSyncCycleAsync();
        second.ErrorCount.Should().Be(0);
        second.PendingCount.Should().Be(0);

        using (var scope = _factory.Services.CreateScope())
        {
            var salesDb = scope.ServiceProvider.GetRequiredService<global::Sales.Infrastructure.Persistence.SalesDbContext>();
            var paid = await salesDb.Orders.IgnoreQueryFilters().CountAsync(o => o.Status == OrderStatus.Paid);
            paid.Should().Be(10);
        }
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var coreContext = scope.ServiceProvider.GetRequiredService<Core.Infrastructure.Persistence.CoreDbContext>();
        await coreContext.Database.EnsureDeletedAsync();
        await coreContext.Database.EnsureCreatedAsync();
        await IntegrationTestDatabaseHelper.MigrateModuleDatabasesAsync(scope);

        var userManager = scope.ServiceProvider.GetRequiredService<
            Microsoft.AspNetCore.Identity.UserManager<Core.Infrastructure.Identity.AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<
            Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole("Admin"));
        }

        var email = $"pdv-offline-{Guid.NewGuid():N}@sysvet.com";
        var user = new Core.Infrastructure.Identity.AppUser { UserName = email, Email = email, TenantId = Guid.NewGuid() };
        await userManager.CreateAsync(user, "Password123!");
        await userManager.AddToRoleAsync(user, "Admin");

        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = "Password123!" });
        loginResponse.EnsureSuccessStatusCode();
        var token = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);
        return client;
    }

    private sealed record LoginResponse(string AccessToken);
}
