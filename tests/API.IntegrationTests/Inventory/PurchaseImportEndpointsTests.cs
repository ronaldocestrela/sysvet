using System.Net.Http.Headers;
using System.Net.Http.Json;
using Core.Infrastructure.Identity;
using FluentAssertions;
using Inventory.Application.PurchaseImports.Commands;
using Inventory.Application.PurchaseImports.Dtos;
using Inventory.Domain.Enums;
using Inventory.Tests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace API.IntegrationTests.Inventory;

[Collection("IntegrationTests")]
public class PurchaseImportEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PurchaseImportEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task ParseAndConfirm_CreatesPurchaseStockMovement()
    {
        var client = await CreateAuthenticatedClientAsync();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var register = new global::Inventory.Application.Products.Commands.RegisterProductCommand(
            "NF Import " + suffix,
            "",
            "SKU-NF-" + suffix,
            "7891234567890",
            "UN",
            5m,
            ProductCategory.Food,
            "23091000",
            null,
            0,
            null,
            null);
        (await client.PostAsJsonAsync("/api/v1/inventory/products", register)).EnsureSuccessStatusCode();

        var preview = await ParseSampleAsync(client);
        var line = preview.Lines.First();
        line.SuggestedProductId.Should().NotBeNull();

        var confirmBody = new ConfirmPurchaseImportRequest
        {
            Supplier = new ConfirmSupplierAction(SupplierConfirmMode.CreateFromEmitter, null),
            Lines =
            [
                new ConfirmLineAction(
                    line.LineId,
                    LineConfirmMode.LinkExisting,
                    line.SuggestedProductId,
                    null)
            ]
        };

        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var confirmResponse = await client.PostAsJsonAsync($"/api/v1/inventory/purchase-imports/{preview.ImportId}/confirm", confirmBody);
        confirmResponse.EnsureSuccessStatusCode();

        var detailResponse = await client.GetAsync($"/api/v1/inventory/purchase-imports/{preview.ImportId}");
        detailResponse.EnsureSuccessStatusCode();
        var detail = await detailResponse.Content.ReadFromJsonAsync<PurchaseImportDetailDto>();
        detail!.Status.Should().Be(PurchaseImportStatus.Confirmed);
        detail.Lines.Should().OnlyContain(l => l.StockMovementId.HasValue);
    }

    [Fact]
    public async Task ParseSameConfirmedKey_ReturnsConflict()
    {
        var client = await CreateAuthenticatedClientAsync();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var register = new global::Inventory.Application.Products.Commands.RegisterProductCommand(
            "NF Dup " + suffix,
            "",
            "SKU-DUP-" + suffix,
            "7891234567890",
            "UN",
            1m,
            ProductCategory.Food,
            "23091000",
            null,
            0,
            null,
            null);
        (await client.PostAsJsonAsync("/api/v1/inventory/products", register)).EnsureSuccessStatusCode();

        var preview = await ParseSampleAsync(client);
        await ConfirmSampleAsync(client, preview);

        await using var stream2 = File.OpenRead(NfeFixturePaths.Resolve("sample-nfeProc.xml"));
        using var form2 = new MultipartFormDataContent();
        form2.Add(new StreamContent(stream2), "file", "sample-nfeProc.xml");
        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var secondParse = await client.PostAsync("/api/v1/inventory/purchase-imports/parse", form2);
        secondParse.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
    }

    private static async Task<PurchaseImportPreviewDto> ParseSampleAsync(HttpClient client)
    {
        await using var xml = File.OpenRead(NfeFixturePaths.Resolve("sample-nfeProc.xml"));
        using var form = new MultipartFormDataContent();
        form.Add(new StreamContent(xml), "file", "sample-nfeProc.xml");
        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var parseResponse = await client.PostAsync("/api/v1/inventory/purchase-imports/parse", form);
        parseResponse.EnsureSuccessStatusCode();
        return (await parseResponse.Content.ReadFromJsonAsync<PurchaseImportPreviewDto>())!;
    }

    private static async Task ConfirmSampleAsync(HttpClient client, PurchaseImportPreviewDto preview)
    {
        var line = preview.Lines.First();
        var confirmBody = new ConfirmPurchaseImportRequest
        {
            Supplier = new ConfirmSupplierAction(SupplierConfirmMode.CreateFromEmitter, null),
            Lines =
            [
                new ConfirmLineAction(
                    line.LineId,
                    LineConfirmMode.LinkExisting,
                    line.SuggestedProductId,
                    null)
            ]
        };

        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        (await client.PostAsJsonAsync($"/api/v1/inventory/purchase-imports/{preview.ImportId}/confirm", confirmBody)).EnsureSuccessStatusCode();
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var coreContext = scope.ServiceProvider.GetRequiredService<Core.Infrastructure.Persistence.CoreDbContext>();
        await coreContext.Database.EnsureDeletedAsync();
        await coreContext.Database.EnsureCreatedAsync();
        await IntegrationTestDatabaseHelper.MigrateModuleDatabasesAsync(scope);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        var user = await userManager.FindByEmailAsync("test@sysvet.com");
        if (user is null)
        {
            user = new AppUser { UserName = "test@sysvet.com", Email = "test@sysvet.com", TenantId = Guid.NewGuid() };
            await userManager.CreateAsync(user, "Password123!");
            await userManager.AddToRoleAsync(user, "Admin");
        }

        var client = _factory.CreateClient();
        var loginResponse = await (await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = "test@sysvet.com", Password = "Password123!" }))
            .Content.ReadFromJsonAsync<LoginResponseDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse!.AccessToken);
        return client;
    }
}
