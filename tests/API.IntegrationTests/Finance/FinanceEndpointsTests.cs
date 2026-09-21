using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Finance.Domain.Enums;
using Inventory.Application.PurchaseImports.Commands;
using Inventory.Application.PurchaseImports.Dtos;
using Inventory.Domain.Enums;
using Inventory.Tests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace API.IntegrationTests.Finance;

[Collection("IntegrationTests")]
public class FinanceEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public FinanceEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task ConfirmPurchaseXml_CreatesPayablesFromDuplicates()
    {
        var client = await CreateAuthenticatedClientAsync();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var register = new global::Inventory.Application.Products.Commands.RegisterProductCommand(
            "NF Fin " + suffix,
            "",
            "SKU-FIN-" + suffix,
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
        var confirmBody = new ConfirmPurchaseImportRequest
        {
            Supplier = new ConfirmSupplierAction(SupplierConfirmMode.CreateFromEmitter, null),
            Lines =
            [
                new ConfirmLineAction(line.LineId, LineConfirmMode.LinkExisting, line.SuggestedProductId, null)
            ]
        };

        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        (await client.PostAsJsonAsync($"/api/v1/inventory/purchase-imports/{preview.ImportId}/confirm", confirmBody)).EnsureSuccessStatusCode();

        using var scope = _factory.Services.CreateScope();
        var financeContext = scope.ServiceProvider.GetRequiredService<global::Finance.Infrastructure.Persistence.FinanceDbContext>();
        var payables = await financeContext.FinancialTitles
            .IgnoreQueryFilters()
            .Where(t => t.SourceId == preview.ImportId)
            .ToListAsync();

        payables.Should().NotBeEmpty();
        payables.Should().OnlyContain(t => t.Direction == TitleDirection.Payable);
        payables.Should().OnlyContain(t => t.Status == TitleStatus.Open);

        var inventoryContext = scope.ServiceProvider.GetRequiredService<global::Inventory.Infrastructure.Persistence.InventoryDbContext>();
        var import = await inventoryContext.PurchaseInvoiceImports
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(i => i.Id == preview.ImportId);
        import.Should().NotBeNull();
        import!.ApIntegrationStatus.Should().Be(ApIntegrationStatus.Linked);
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

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var coreContext = scope.ServiceProvider.GetRequiredService<Core.Infrastructure.Persistence.CoreDbContext>();
        await coreContext.Database.EnsureDeletedAsync();
        await coreContext.Database.MigrateAsync();
        await IntegrationTestDatabaseHelper.MigrateModuleDatabasesAsync(scope);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Core.Infrastructure.Identity.AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        var email = $"finance-{Guid.NewGuid():N}@sysvet.com";
        var user = new Core.Infrastructure.Identity.AppUser { UserName = email, Email = email, TenantId = Guid.NewGuid() };
        await userManager.CreateAsync(user, "Password123!");
        await userManager.AddToRoleAsync(user, "Admin");

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = "Password123!" });
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse!.AccessToken);
        return client;
    }

    private sealed record LoginResponseDto(string AccessToken);
}
