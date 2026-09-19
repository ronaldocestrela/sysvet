using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Inventory.Application.Products.Commands;
using Inventory.Application.StockMovements.Commands;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sales.Application.Orders.Commands;
using Sales.Application.Orders.Dtos;
using Sales.Domain.Enums;
using Xunit;

namespace API.IntegrationTests.Sales;

[Collection("IntegrationTests")]
public class SalesEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SalesEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        using var scope = _factory.Services.CreateScope();

        var coreContext = scope.ServiceProvider.GetRequiredService<Core.Infrastructure.Persistence.CoreDbContext>();
        await coreContext.Database.EnsureDeletedAsync();
        await coreContext.Database.EnsureCreatedAsync();

        var salesContext = scope.ServiceProvider.GetRequiredService<global::Sales.Infrastructure.Persistence.SalesDbContext>();
        await salesContext.Database.MigrateAsync();

        var inventoryContext = scope.ServiceProvider.GetRequiredService<global::Inventory.Infrastructure.Persistence.InventoryDbContext>();
        await inventoryContext.Database.MigrateAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Core.Infrastructure.Identity.AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in new[] { "Admin" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var email = $"sales-{Guid.NewGuid():N}@sysvet.com";
        var user = new Core.Infrastructure.Identity.AppUser { UserName = email, Email = email, TenantId = Guid.NewGuid() };
        await userManager.CreateAsync(user, "Password123!");
        await userManager.AddToRoleAsync(user, "Admin");

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = "Password123!" });
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse!.AccessToken);
        return client;
    }

    [Fact]
    public async Task OpenCashRegister_ShouldReturnSuccess()
    {
        var client = await CreateAuthenticatedClientAsync();
        var response = await client.PostAsJsonAsync("/api/v1/sales/cash-registers/open", new { openingBalance = 100m });
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task PayOrder_ShouldDebitStockAndMarkFinancePending()
    {
        var client = await CreateAuthenticatedClientAsync();
        var suffix = Guid.NewGuid().ToString()[..8];

        var productResponse = await client.PostAsJsonAsync("/api/v1/inventory/products", new RegisterProductCommand(
            "PDV Prod " + suffix,
            "",
            "PDV-" + suffix,
            "789" + suffix,
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

        var stockIn = await client.PostAsJsonAsync("/api/v1/inventory/stock/movements", new
        {
            productId,
            type = MovementType.In,
            quantity = 10m,
            reason = "Opening"
        });
        stockIn.EnsureSuccessStatusCode();

        var openRegister = await client.PostAsJsonAsync("/api/v1/sales/cash-registers/open", new { openingBalance = 50m });
        openRegister.EnsureSuccessStatusCode();
        var registerId = await openRegister.Content.ReadFromJsonAsync<Guid>();

        var createOrder = await client.PostAsJsonAsync("/api/v1/sales/orders", new CreateOrderCommand
        {
            CashRegisterId = registerId,
            Items =
            [
                new CreateOrderItemDto
                {
                    Kind = OrderItemKind.Product,
                    ProductId = productId,
                    ProductName = "PDV Prod",
                    Quantity = 2m,
                    UnitPrice = 25m
                },
                new CreateOrderItemDto
                {
                    Kind = OrderItemKind.Service,
                    ProductName = "Consulta",
                    Quantity = 1m,
                    UnitPrice = 50m
                }
            ]
        });
        if (!createOrder.IsSuccessStatusCode)
        {
            var body = await createOrder.Content.ReadAsStringAsync();
            throw new Exception($"Create order failed: {createOrder.StatusCode} {body}");
        }
        var orderId = await createOrder.Content.ReadFromJsonAsync<Guid>();

        var pay = await client.PostAsJsonAsync($"/api/v1/sales/orders/{orderId}/pay", new PayOrderCommand
        {
            OrderId = orderId,
            Payments =
            [
                new PayOrderPaymentDto { Method = PaymentMethod.Cash, Amount = 50m },
                new PayOrderPaymentDto { Method = PaymentMethod.Pix, Amount = 50m }
            ]
        });
        if (!pay.IsSuccessStatusCode)
        {
            var payBody = await pay.Content.ReadAsStringAsync();
            throw new Exception($"Pay failed: {pay.StatusCode} {payBody}");
        }

        var getOrder = await client.GetAsync($"/api/v1/sales/orders/{orderId}");
        getOrder.EnsureSuccessStatusCode();
        var order = await getOrder.Content.ReadFromJsonAsync<OrderDetailDto>();
        order!.Status.Should().Be(OrderStatus.Paid);
        order.FinanceIntegrationStatus.Should().Be(FinanceIntegrationStatus.Pending);
        order.Payments.Should().HaveCount(2);

        using var scope = _factory.Services.CreateScope();
        var inventoryContext = scope.ServiceProvider.GetRequiredService<global::Inventory.Infrastructure.Persistence.InventoryDbContext>();
        var saleMovements = await inventoryContext.StockMovements
            .IgnoreQueryFilters()
            .Where(m => m.ProductId == productId && m.Reason.Contains("Sale"))
            .ToListAsync();
        saleMovements.Should().NotBeEmpty();
        saleMovements.Sum(m => m.Quantity).Should().Be(2m);
    }
}

public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
}
