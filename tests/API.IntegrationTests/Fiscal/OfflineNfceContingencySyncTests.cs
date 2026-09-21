using System.Net.Http.Headers;
using System.Net.Http.Json;
using Clients.Infrastructure.Fiscal;
using Clients.Infrastructure.Http;
using Clients.Infrastructure.Sales;
using Clients.Infrastructure.Sync;
using API.IntegrationTests.Sync;
using Fiscal.Application.Documents;
using Fiscal.Application.Issuer;
using Fiscal.Domain.Enums;
using FluentAssertions;
using Inventory.Application.Products.Commands;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sales.Domain.Payments;
using Xunit;

namespace API.IntegrationTests.Fiscal;

[Collection("IntegrationTests")]
public class OfflineNfceContingencySyncTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public OfflineNfceContingencySyncTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task OfflineSale_EmitsNfceInContingency_TransmitsAfterSync()
    {
        var apiClient = await CreateAuthenticatedClientAsync();
        await SeedIssuerAsync(apiClient);

        await using var harness = await SyncPocHarness.CreateAsync(apiClient);
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var productId = Guid.NewGuid();
        var productResponse = await apiClient.PostAsJsonAsync("/api/v1/inventory/products", new RegisterProductCommand(
            "NFC-e " + suffix,
            "",
            "NFCE-" + suffix,
            "888" + suffix,
            "UN",
            0m,
            ProductCategory.Food,
            "23091000",
            null,
            0,
            null,
            RequiresLot: null,
            ProductId: productId));
        productResponse.EnsureSuccessStatusCode();

        await apiClient.PostAsJsonAsync("/api/v1/inventory/stock/movements", new
        {
            productId,
            type = MovementType.In,
            quantity = 50m,
            reason = "Seed"
        }).ContinueWith(t => t.Result.EnsureSuccessStatusCode());

        var localProduct = Product.Create("NFC-e local", "d", "NFCE-L-" + suffix, "888" + suffix, "UN", 0, ProductCategory.Food, "23091000", null, 0, null, id: productId);
        localProduct.IsSuccess.Should().BeTrue();
        harness.OfflineDb.Products.Add(localProduct.Value);
        harness.OfflineDb.ProductBalances.Add(new ProductBalance(productId, 50m));
        await harness.OfflineDb.SaveChangesAsync();

        harness.OfflineDb.FiscalIssuerCache.Add(new OfflineFiscalIssuerCache
        {
            LegalName = "Clinic",
            TradeName = "Clinic",
            Cnpj = "12345678000195",
            State = "SP",
            IbgeCityCode = 3550308,
            NfceSeries = 1,
            HasCertificate = true,
            EncryptedPfxBase64 = "fake",
            EncryptedCertificatePassword = "enc",
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await harness.OfflineDb.SaveChangesAsync();

        var wake = new SyncWakeSignal();
        var connectivity = new FakeSyncConnectivity();
        connectivity.SetOnline(true);
        var salesStore = new OfflineSalesStore(
            harness.OfflineDb,
            wake,
            connectivity,
            new SimulatedPaymentTerminal(),
            new OfflineFiscalNfceService(harness.OfflineDb));

        var register = await salesStore.OpenCashRegisterAsync(10m);
        register.IsSuccess.Should().BeTrue();

        var sale = await salesStore.CreateAndPayOrderAsync(
            new CreateSalesOrderClientRequest
            {
                CashRegisterId = register.Value,
                Items =
                [
                    new SalesOrderItemClientDto
                    {
                        Kind = "Product",
                        ProductId = productId,
                        ProductName = "Ração",
                        Quantity = 1,
                        UnitPrice = 10m
                    }
                ]
            },
            [new PayOrderPaymentClientDto { Method = "Cash", Amount = 10m }]);
        sale.IsSuccess.Should().BeTrue();

        var outbox = (await harness.OfflineDb.OutboxMessages.ToListAsync()).OrderBy(o => o.CreatedAt).ToList();
        SyncPocMetrics last = null!;
        for (var i = 0; i < 8; i++)
        {
            last = await harness.RunSyncCycleAsync();
            if (last.PendingCount == 0)
            {
                break;
            }

            await Task.Delay(200);
        }

        last.ErrorCount.Should().Be(0);
        last.PendingCount.Should().Be(0);

        var deadLetters = await harness.OfflineDb.OutboxMessages.Where(m => m.Error != null).ToListAsync();
        deadLetters.Should().BeEmpty(string.Join("; ", deadLetters.Select(d => d.Error)));

        using var scope = _factory.Services.CreateScope();
        var fiscalDb = scope.ServiceProvider.GetRequiredService<global::Fiscal.Infrastructure.Persistence.FiscalDbContext>();
        var allDocs = await fiscalDb.FiscalDocuments.IgnoreQueryFilters().ToListAsync();
        var doc = allDocs.FirstOrDefault(d => d.SourceOrderId == sale.Value);
        doc.Should().NotBeNull($"expected fiscal doc for order {sale.Value}, found {allDocs.Count} docs");
        doc!.Status.Should().Be(FiscalDocumentStatus.Authorized);
    }

    private static async Task SeedIssuerAsync(HttpClient client)
    {
        var upsert = new UpsertIssuerProfileCommand(
            LegalName: "Clinic LTDA",
            TradeName: "Clinic",
            Cnpj: "11222333000181",
            StateRegistration: "123",
            MunicipalRegistration: "456",
            Cnae: "7500100",
            Street: "Rua A",
            Number: "100",
            Complement: null,
            District: "Centro",
            City: "São Paulo",
            State: "SP",
            PostalCode: "01310100",
            IbgeCityCode: 3550308,
            Phone: "11999999999",
            NationalServiceTaxCode: "0107",
            DefaultIssRate: 2m,
            Environment: FiscalEnvironment.Homologation);
        (await client.PutAsJsonAsync("/api/v1/fiscal/issuer", upsert)).EnsureSuccessStatusCode();

        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent([0x01, 0x02, 0x03]), "file", "cert.pfx");
        form.Add(new StringContent("test-password"), "password");
        (await client.PostAsync("/api/v1/fiscal/issuer/certificate", form)).EnsureSuccessStatusCode();
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var core = scope.ServiceProvider.GetRequiredService<Core.Infrastructure.Persistence.CoreDbContext>();
        await core.Database.EnsureDeletedAsync();
        await core.Database.EnsureCreatedAsync();
        await IntegrationTestDatabaseHelper.MigrateModuleDatabasesAsync(scope);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Core.Infrastructure.Identity.AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        var email = $"nfce-offline-{Guid.NewGuid():N}@sysvet.com";
        var user = new Core.Infrastructure.Identity.AppUser { UserName = email, Email = email, TenantId = Guid.NewGuid() };
        await userManager.CreateAsync(user, "Password123!");
        await userManager.AddToRoleAsync(user, "Admin");

        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = "Password123!" });
        var token = await login.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);
        return client;
    }

    private sealed record LoginResponse(string AccessToken);
}
