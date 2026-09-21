using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Finance.Application.Reconciliation.Dtos;
using Finance.Application.Reports.Dtos;
using Finance.Domain.Enums;
using Inventory.Application.PurchaseImports.Commands;
using Inventory.Application.PurchaseImports.Dtos;
using Inventory.Application.StockMovements.Commands;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Sales.Application.Orders.Commands;
using Sales.Application.Orders.Dtos;
using Sales.Domain.Enums;
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
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

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

    [Fact]
    public async Task ImportCardStatement_MatchesReceivableByNsu()
    {
        var client = await CreateAuthenticatedClientAsync();
        var suffix = Guid.NewGuid().ToString()[..8];

        var productResponse = await client.PostAsJsonAsync("/api/v1/inventory/products", new global::Inventory.Application.Products.Commands.RegisterProductCommand(
            "Card " + suffix,
            "",
            "CD-" + suffix,
            "789" + suffix,
            "UN",
            0m,
            ProductCategory.Food,
            "23091000",
            null,
            0,
            null,
            null));
        var productId = await productResponse.Content.ReadFromJsonAsync<Guid>();

        await client.PostAsJsonAsync("/api/v1/inventory/stock/movements", new
        {
            productId,
            type = MovementType.In,
            quantity = 5m,
            reason = "Opening"
        });

        var registerId = await (await client.PostAsJsonAsync("/api/v1/sales/cash-registers/open", new { openingBalance = 0m }))
            .Content.ReadFromJsonAsync<Guid>();

        var orderId = await (await client.PostAsJsonAsync("/api/v1/sales/orders", new CreateOrderCommand
        {
            CashRegisterId = registerId,
            Items =
            [
                new CreateOrderItemDto
                {
                    Kind = OrderItemKind.Product,
                    ProductId = productId,
                    ProductName = "Card item",
                    Quantity = 1m,
                    UnitPrice = 80m
                }
            ]
        })).Content.ReadFromJsonAsync<Guid>();

        (await client.PostAsJsonAsync($"/api/v1/sales/orders/{orderId}/pay", new PayOrderCommand
        {
            OrderId = orderId,
            Payments =
            [
                new PayOrderPaymentDto
                {
                    Method = PaymentMethod.CreditCard,
                    Amount = 80m
                }
            ]
        })).EnsureSuccessStatusCode();

        var order = await (await client.GetAsync($"/api/v1/sales/orders/{orderId}"))
            .Content.ReadFromJsonAsync<OrderDetailDto>();
        var nsu = order!.Payments.Single().Nsu;
        nsu.Should().NotBeNullOrWhiteSpace();

        using (var scope = _factory.Services.CreateScope())
        {
            var financeContext = scope.ServiceProvider.GetRequiredService<global::Finance.Infrastructure.Persistence.FinanceDbContext>();
            var allocation = await financeContext.TitleAllocations
                .IgnoreQueryFilters()
                .FirstAsync(a => a.ExternalReference == nsu);
            allocation.ExternalReference.Should().Be(nsu);
        }

        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var importResponse = await client.PostAsJsonAsync("/api/v1/finance/card-reconciliations", new
        {
            reference = "Test batch",
            periodFrom = DateOnly.FromDateTime(DateTime.UtcNow),
            periodTo = DateOnly.FromDateTime(DateTime.UtcNow),
            lines = new[]
            {
                new { nsu, amount = 80m, method = "CreditCard" }
            }
        });
        importResponse.EnsureSuccessStatusCode();
        var batchId = await importResponse.Content.ReadFromJsonAsync<Guid>();

        var detail = await (await client.GetAsync($"/api/v1/finance/card-reconciliations/{batchId}"))
            .Content.ReadFromJsonAsync<CardReconciliationBatchDetailDto>();
        detail!.Lines.Should().ContainSingle(l => l.Status == CardReconciliationLineStatus.Matched);

        var unmatched = await (await client.GetAsync("/api/v1/finance/card-settlements/unmatched"))
            .Content.ReadFromJsonAsync<List<UnmatchedCardSettlementDto>>();
        unmatched!.Should().NotContain(u => u.Nsu == nsu);
    }

    [Fact]
    public async Task MonthlyStatements_MatchAccountsPayableReceivable()
    {
        var client = await CreateAuthenticatedClientAsync();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var monthStart = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var productResponse = await client.PostAsJsonAsync("/api/v1/inventory/products", new global::Inventory.Application.Products.Commands.RegisterProductCommand(
            "Stmt " + suffix,
            "",
            "ST-" + suffix,
            "7891234567890",
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

        await client.PostAsJsonAsync("/api/v1/inventory/stock/movements", new
        {
            productId,
            type = MovementType.In,
            quantity = 10m,
            reason = "Opening"
        });

        var preview = await ParseSampleAsync(client);
        var line = preview.Lines.First();
        var confirmBody = new ConfirmPurchaseImportRequest
        {
            Supplier = new ConfirmSupplierAction(SupplierConfirmMode.CreateFromEmitter, null),
            Lines =
            [
                new ConfirmLineAction(line.LineId, LineConfirmMode.LinkExisting, productId, null)
            ]
        };
        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        (await client.PostAsJsonAsync($"/api/v1/inventory/purchase-imports/{preview.ImportId}/confirm", confirmBody)).EnsureSuccessStatusCode();

        decimal payableTotal;
        Guid payableId;
        using (var scope = _factory.Services.CreateScope())
        {
            var financeContext = scope.ServiceProvider.GetRequiredService<global::Finance.Infrastructure.Persistence.FinanceDbContext>();
            var payables = await financeContext.FinancialTitles
                .IgnoreQueryFilters()
                .Where(t => t.SourceId == preview.ImportId && t.Direction == TitleDirection.Payable)
                .ToListAsync();
            payables.Should().NotBeEmpty();
            payableTotal = payables.Sum(t => t.OriginalAmount);
            payableId = payables.First().Id;
        }

        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var openRegister = await client.PostAsJsonAsync("/api/v1/sales/cash-registers/open", new { openingBalance = 0m });
        openRegister.EnsureSuccessStatusCode();
        var registerId = await openRegister.Content.ReadFromJsonAsync<Guid>();

        const decimal saleTotal = 100m;
        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var createOrder = await client.PostAsJsonAsync("/api/v1/sales/orders", new CreateOrderCommand
        {
            CashRegisterId = registerId,
            Items =
            [
                new CreateOrderItemDto
                {
                    Kind = OrderItemKind.Product,
                    ProductId = productId,
                    ProductName = "Stmt item",
                    Quantity = 1m,
                    UnitPrice = saleTotal
                }
            ]
        });
        createOrder.EnsureSuccessStatusCode();
        var orderId = await createOrder.Content.ReadFromJsonAsync<Guid>();

        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        (await client.PostAsJsonAsync($"/api/v1/sales/orders/{orderId}/pay", new PayOrderCommand
        {
            OrderId = orderId,
            Payments = [new PayOrderPaymentDto { Method = PaymentMethod.Cash, Amount = saleTotal }]
        })).EnsureSuccessStatusCode();

        var cashFlow1 = await (await client.GetAsync(
                $"/api/v1/finance/cash-flow?from={monthStart:yyyy-MM-dd}&to={monthEnd:yyyy-MM-dd}"))
            .Content.ReadFromJsonAsync<CashFlowReportDto>(JsonOptions);
        cashFlow1!.TotalRealizedInflow.Should().Be(saleTotal);
        cashFlow1.TotalRealizedOutflow.Should().Be(0m);

        var dre1 = await (await client.GetAsync(
                $"/api/v1/finance/dre?year={monthStart.Year}&month={monthStart.Month}"))
            .Content.ReadFromJsonAsync<SimplifiedDreReportDto>(JsonOptions);
        dre1!.TotalRevenue.Should().Be(saleTotal);
        dre1.TotalExpense.Should().Be(payableTotal);

        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        (await client.PostAsJsonAsync($"/api/v1/financial-titles/{payableId}/settle", new { amount = payableTotal, method = "Pix" }))
            .EnsureSuccessStatusCode();

        var cashFlow2 = await (await client.GetAsync(
                $"/api/v1/finance/cash-flow?from={monthStart:yyyy-MM-dd}&to={monthEnd:yyyy-MM-dd}"))
            .Content.ReadFromJsonAsync<CashFlowReportDto>(JsonOptions);
        cashFlow2!.TotalRealizedOutflow.Should().Be(payableTotal);

        var dre2 = await (await client.GetAsync(
                $"/api/v1/finance/dre?year={monthStart.Year}&month={monthStart.Month}"))
            .Content.ReadFromJsonAsync<SimplifiedDreReportDto>(JsonOptions);
        dre2!.TotalRevenue.Should().Be(dre1.TotalRevenue);
        dre2.TotalExpense.Should().Be(dre1.TotalExpense);
    }

    [Fact]
    public async Task ExportMonthlyStatements_ReturnsCsv()
    {
        var client = await CreateAuthenticatedClientAsync();
        var start = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        var response = await client.GetAsync(
            $"/api/v1/finance/statements/export?from={start:yyyy-MM-dd}&to={end:yyyy-MM-dd}&format=Csv");
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
        var text = await response.Content.ReadAsStringAsync();
        text.Should().Contain("Fluxo de caixa");
        text.Should().Contain("DRE simplificada");
    }

    [Fact]
    public async Task ExportMonthlyStatements_ReturnsPdf()
    {
        var client = await CreateAuthenticatedClientAsync();
        var start = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        var response = await client.GetAsync(
            $"/api/v1/finance/statements/export?from={start:yyyy-MM-dd}&to={end:yyyy-MM-dd}&format=Pdf");
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
        bytes[0].Should().Be(0x25);
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
