using System.Net.Http.Headers;
using System.Net.Http.Json;
using Core.Application.Pets.Commands;
using Core.Application.Tutors.Commands;
using Core.Domain.ValueObjects;
using Fiscal.Application.Documents;
using Fiscal.Application.Issuer;
using Fiscal.Application.Planning.Dtos;
using Fiscal.Domain.Enums;
using FluentAssertions;
using Inventory.Application.Products.Commands;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sales.Application.CashRegisters.Dtos;
using Sales.Application.Orders.Commands;
using Sales.Application.Orders.Dtos;
using Sales.Domain.Enums;
using Xunit;

namespace API.IntegrationTests.Fiscal;

[Collection("IntegrationTests")]
public class FiscalEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public FiscalEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task IssueFromOrder_AuthorizesNfe_WithFakeGateway()
    {
        var client = await CreateAuthenticatedClientAsync();
        var suffix = Guid.NewGuid().ToString()[..8];

        await SeedIssuerAsync(client);

        var tutorId = Guid.NewGuid();
        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        (await client.PostAsJsonAsync("/api/v1/tutors", new CreateTutorCommand(tutorId, "Fiscal Tutor", $"fiscal-{suffix}@sysvet.com", "52998224725", "11999999999"))).EnsureSuccessStatusCode();

        using (var scope = _factory.Services.CreateScope())
        {
            var core = scope.ServiceProvider.GetRequiredService<Core.Infrastructure.Persistence.CoreDbContext>();
            var tutor = await core.Tutors.IgnoreQueryFilters().FirstAsync(t => t.Id == tutorId);
            var address = PostalAddress.Create("Rua Teste", "100", null, "Centro", "São Paulo", "SP", "01310100", 3550308).Value;
            tutor.SetAddress(address);
            await core.SaveChangesAsync();
        }

        var petId = Guid.NewGuid();
        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        (await client.PostAsJsonAsync("/api/v1/pets", new CreatePetCommand("Rex", Core.Domain.Entities.PetSpecies.Dog, "Labrador", Core.Domain.Entities.PetSex.Male, tutorId, null, petId))).EnsureSuccessStatusCode();

        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var productResponse = await client.PostAsJsonAsync("/api/v1/inventory/products", new RegisterProductCommand(
            "Fiscal " + suffix,
            "",
            "FIS-" + suffix,
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
        var productBody = await productResponse.Content.ReadAsStringAsync();
        var productId = Guid.Parse(productBody.Trim().Trim('"'));

        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        (await client.PostAsJsonAsync("/api/v1/inventory/stock/movements", new
        {
            productId,
            type = MovementType.In,
            quantity = 5m,
            reason = "Opening"
        })).EnsureSuccessStatusCode();

        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var openRegisterResponse = await client.PostAsJsonAsync("/api/v1/sales/cash-registers/open", new { openingBalance = 0m });
        openRegisterResponse.EnsureSuccessStatusCode();
        var registerBody = await openRegisterResponse.Content.ReadAsStringAsync();
        var registerId = registerBody.Trim().Trim('"').Length == 36
            ? Guid.Parse(registerBody.Trim().Trim('"'))
            : (await (await client.GetAsync("/api/v1/sales/cash-registers/open")).Content.ReadFromJsonAsync<OpenCashRegisterDto>())!.Id;

        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var createOrderResponse = await client.PostAsJsonAsync("/api/v1/sales/orders", new CreateOrderCommand
        {
            CashRegisterId = registerId,
            TutorId = tutorId,
            PetId = petId,
            Items =
            [
                new CreateOrderItemDto
                {
                    Kind = OrderItemKind.Product,
                    ProductId = productId,
                    ProductName = "Fiscal Prod",
                    Quantity = 1m,
                    UnitPrice = 10m
                }
            ]
        });
        if (!createOrderResponse.IsSuccessStatusCode)
        {
            throw new Exception($"Create order failed: {createOrderResponse.StatusCode} {await createOrderResponse.Content.ReadAsStringAsync()}");
        }

        var orderBody = await createOrderResponse.Content.ReadAsStringAsync();
        var orderId = Guid.Parse(orderBody.Trim().Trim('"'));

        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var payResponse = await client.PostAsJsonAsync($"/api/v1/sales/orders/{orderId}/pay", new PayOrderCommand
        {
            OrderId = orderId,
            Payments = [new PayOrderPaymentDto { Method = PaymentMethod.Cash, Amount = 10m }]
        });
        if (!payResponse.IsSuccessStatusCode)
        {
            throw new Exception($"Pay failed: {payResponse.StatusCode} {await payResponse.Content.ReadAsStringAsync()}");
        }

        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var issueResponse = await client.PostAsJsonAsync("/api/v1/fiscal-documents/from-order", new { orderId });
        if (!issueResponse.IsSuccessStatusCode)
        {
            throw new Exception($"Issue failed: {issueResponse.StatusCode} {await issueResponse.Content.ReadAsStringAsync()}");
        }
        var documentIds = await issueResponse.Content.ReadFromJsonAsync<List<Guid>>();
        documentIds.Should().NotBeNull().And.NotBeEmpty();

        var detailResponse = await client.GetAsync($"/api/v1/fiscal-documents/{documentIds![0]}");
        detailResponse.EnsureSuccessStatusCode();
        var detail = await detailResponse.Content.ReadFromJsonAsync<FiscalDocumentDetailDto>();
        detail.Should().NotBeNull();
        detail!.Status.Should().Be(FiscalDocumentStatus.Authorized);
        detail.AccessKey.Should().NotBeNullOrEmpty();

        var xmlResponse = await client.GetAsync($"/api/v1/fiscal-documents/{documentIds[0]}/xml");
        xmlResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task PeriodTaxReport_MatchesAuthorizedDocuments()
    {
        var client = await CreateAuthenticatedClientAsync();
        var suffix = Guid.NewGuid().ToString()[..8];
        await SeedIssuerAsync(client);

        var tutorId = Guid.NewGuid();
        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        (await client.PostAsJsonAsync("/api/v1/tutors", new CreateTutorCommand(tutorId, "Planning Tutor", $"plan-{suffix}@sysvet.com", "52998224725", "11999999999"))).EnsureSuccessStatusCode();

        using (var scope = _factory.Services.CreateScope())
        {
            var core = scope.ServiceProvider.GetRequiredService<Core.Infrastructure.Persistence.CoreDbContext>();
            var tutor = await core.Tutors.IgnoreQueryFilters().FirstAsync(t => t.Id == tutorId);
            var address = PostalAddress.Create("Rua Teste", "100", null, "Centro", "São Paulo", "SP", "01310100", 3550308).Value;
            tutor.SetAddress(address);
            await core.SaveChangesAsync();
        }

        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var productResponse = await client.PostAsJsonAsync("/api/v1/inventory/products", new RegisterProductCommand(
            "Plan " + suffix,
            "",
            "PLN-" + suffix,
            "788" + suffix,
            "UN",
            0m,
            ProductCategory.Food,
            "23091000",
            null,
            0,
            null,
            null));
        productResponse.EnsureSuccessStatusCode();
        var productId = Guid.Parse((await productResponse.Content.ReadAsStringAsync()).Trim().Trim('"'));

        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        (await client.PostAsJsonAsync("/api/v1/inventory/stock/movements", new
        {
            productId,
            type = MovementType.In,
            quantity = 5m,
            reason = "Opening"
        })).EnsureSuccessStatusCode();

        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var openRegisterResponse = await client.PostAsJsonAsync("/api/v1/sales/cash-registers/open", new { openingBalance = 0m });
        openRegisterResponse.EnsureSuccessStatusCode();
        var registerId = Guid.Parse((await openRegisterResponse.Content.ReadAsStringAsync()).Trim().Trim('"'));

        const decimal goodsTotal = 50m;
        const decimal servicesTotal = 80m;
        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var createOrderResponse = await client.PostAsJsonAsync("/api/v1/sales/orders", new CreateOrderCommand
        {
            CashRegisterId = registerId,
            TutorId = tutorId,
            Items =
            [
                new CreateOrderItemDto
                {
                    Kind = OrderItemKind.Product,
                    ProductId = productId,
                    ProductName = "Plan Prod",
                    Quantity = 2m,
                    UnitPrice = 25m
                },
                new CreateOrderItemDto
                {
                    Kind = OrderItemKind.Service,
                    ProductName = "Consulta",
                    Quantity = 1m,
                    UnitPrice = servicesTotal
                }
            ]
        });
        createOrderResponse.EnsureSuccessStatusCode();
        var orderId = Guid.Parse((await createOrderResponse.Content.ReadAsStringAsync()).Trim().Trim('"'));

        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        (await client.PostAsJsonAsync($"/api/v1/sales/orders/{orderId}/pay", new PayOrderCommand
        {
            OrderId = orderId,
            Payments = [new PayOrderPaymentDto { Method = PaymentMethod.Cash, Amount = goodsTotal + servicesTotal }]
        })).EnsureSuccessStatusCode();

        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        (await client.PostAsJsonAsync("/api/v1/fiscal-documents/from-order", new { orderId })).EnsureSuccessStatusCode();

        var monthStart = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        var planningResponse = await client.GetAsync(
            $"/api/v1/fiscal/planning?from={monthStart:yyyy-MM-dd}&to={monthEnd:yyyy-MM-dd}");
        planningResponse.EnsureSuccessStatusCode();
        var planning = await planningResponse.Content.ReadFromJsonAsync<FiscalPlanningReportDto>();
        planning.Should().NotBeNull();
        planning!.Period.GoodsRevenue.Should().Be(goodsTotal);
        planning.Period.ServicesRevenue.Should().Be(servicesTotal);
        planning.Period.TotalGrossRevenue.Should().Be(goodsTotal + servicesTotal);
        planning.Period.EstimatedIss.Should().Be(Math.Round(servicesTotal * 0.02m, 2));
        planning.Period.AuthorizedDocumentCount.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task ExportFiscalPlanning_ReturnsCsv()
    {
        var client = await CreateAuthenticatedClientAsync();
        await SeedIssuerAsync(client);
        var start = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        var response = await client.GetAsync(
            $"/api/v1/fiscal/planning/export?from={start:yyyy-MM-dd}&to={end:yyyy-MM-dd}&format=csv");
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
        var text = await response.Content.ReadAsStringAsync();
        text.Should().Contain("Apuração do período");
        text.Should().Contain("Simulação de enquadramento");
    }

    [Fact]
    public async Task ExportFiscalPlanning_ReturnsPdf()
    {
        var client = await CreateAuthenticatedClientAsync();
        await SeedIssuerAsync(client);
        var start = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        var response = await client.GetAsync(
            $"/api/v1/fiscal/planning/export?from={start:yyyy-MM-dd}&to={end:yyyy-MM-dd}&format=pdf");
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes[0].Should().Be(0x25);
    }

    private static async Task SeedIssuerAsync(HttpClient client)
    {
        var upsert = new UpsertIssuerProfileCommand(
            LegalName: "Clínica Teste LTDA",
            TradeName: "Clínica Teste",
            Cnpj: "11222333000181",
            StateRegistration: "123456789",
            MunicipalRegistration: "987654",
            Cnae: "7500100",
            Street: "Av Paulista",
            Number: "1000",
            Complement: null,
            District: "Bela Vista",
            City: "São Paulo",
            State: "SP",
            PostalCode: "01310100",
            IbgeCityCode: 3550308,
            Phone: "1133334444",
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
        await IntegrationTestDatabaseHelper.ResetModuleDatabasesAsync(scope);
        await core.Database.MigrateAsync();
        await IntegrationTestDatabaseHelper.MigrateModuleDatabasesAsync(scope);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Core.Infrastructure.Identity.AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        var email = $"fiscal-{Guid.NewGuid():N}@sysvet.com";
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
