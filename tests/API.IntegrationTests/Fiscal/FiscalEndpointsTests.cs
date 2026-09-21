using System.Net.Http.Headers;
using System.Net.Http.Json;
using Core.Application.Pets.Commands;
using Core.Application.Tutors.Commands;
using Core.Domain.ValueObjects;
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
