using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Core.Application.Authorization;
using Core.Infrastructure.Identity;
using Core.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Billing;
using Platform.Application.Tenants.Dtos;
using Platform.Domain.Entities;
using Platform.Infrastructure.Persistence;
using Xunit;

namespace API.IntegrationTests.Platform;

[Collection("IntegrationTests")]
public class PlatformBillingEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Password = "Password123!";
    private const string ValidCpf = "39053344705";

    private readonly WebApplicationFactory<Program> _factory;

    public PlatformBillingEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task RecurringSubscription_Charged_WhenPeriodDue()
    {
        var superAdmin = await CreateSuperAdminClientAsync();
        var (tenantId, _) = await OnboardClinicAsync(superAdmin);
        await SetupBillingProfileAsync(superAdmin, tenantId);
        await SetSubscriptionPeriodDueAsync(tenantId);

        var charge = await superAdmin.PostAsync($"/api/v1/platform/tenants/{tenantId}/billing/charge", null);
        charge.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await charge.Content.ReadFromJsonAsync<ChargeTenantBillingResultDto>();
        body.Should().NotBeNull();
        body!.Amount.Should().BeGreaterThan(0);
        body.GatewayPaymentId.Should().NotBeNullOrEmpty();

        await using var scope = _factory.Services.CreateAsyncScope();
        var platform = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        var openInvoices = await platform.BillingInvoices
            .Where(i => i.TenantId == tenantId && i.Status == BillingInvoiceStatus.Open)
            .ToListAsync();
        openInvoices.Should().ContainSingle();
    }

    [Fact]
    public async Task PaymentWebhook_UpdatesBillingStanding_WhenPaid()
    {
        var superAdmin = await CreateSuperAdminClientAsync();
        var (tenantId, _) = await OnboardClinicAsync(superAdmin);
        await SetupBillingProfileAsync(superAdmin, tenantId);
        await SetSubscriptionPeriodDueAsync(tenantId);

        var charge = await superAdmin.PostAsync($"/api/v1/platform/tenants/{tenantId}/billing/charge", null);
        var chargeBody = await charge.Content.ReadFromJsonAsync<ChargeTenantBillingResultDto>();
        var invoiceId = chargeBody!.InvoiceId;
        var paymentId = chargeBody.GatewayPaymentId!;

        using var webhookClient = _factory.CreateClient();
        var webhook = await webhookClient.PostAsJsonAsync("/api/v1/platform/webhooks/asaas", new
        {
            @event = "PAYMENT_CONFIRMED",
            payment = new { id = paymentId, externalReference = invoiceId.ToString() }
        });
        webhook.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        await using var scope = _factory.Services.CreateAsyncScope();
        var platform = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        var subscription = await platform.TenantSubscriptions.FirstAsync(s => s.TenantId == tenantId);
        subscription.BillingStanding.Should().Be(BillingStanding.Good);

        var invoice = await platform.BillingInvoices.FirstAsync(i => i.Id == invoiceId);
        invoice.Status.Should().Be(BillingInvoiceStatus.Paid);
    }

    private async Task SetupBillingProfileAsync(HttpClient superAdmin, Guid tenantId)
    {
        var customer = await superAdmin.PutAsJsonAsync(
            $"/api/v1/platform/tenants/{tenantId}/billing/customer",
            new { Name = "Clínica Teste", Email = "billing@test.com", CpfCnpj = ValidCpf });
        customer.StatusCode.Should().Be(HttpStatusCode.OK);

        var method = await superAdmin.PutAsJsonAsync(
            $"/api/v1/platform/tenants/{tenantId}/billing/payment-method",
            new { Kind = BillingPaymentMethodKind.Pix, CreditCardToken = (string?)null });
        method.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task SetSubscriptionPeriodDueAsync(Guid tenantId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var platform = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        var past = DateTimeOffset.UtcNow.AddDays(-1);
        await platform.TenantSubscriptions
            .Where(s => s.TenantId == tenantId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.PeriodEnd, past)
                .SetProperty(s => s.Status, SubscriptionStatus.Active));
    }

    private async Task<HttpClient> CreateSuperAdminClientAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
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

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        const string email = "superadmin-bill@vetnexus.app";
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            await userManager.DeleteAsync(existing);
        }

        var user = new AppUser
        {
            UserName = email,
            Email = email,
            TenantId = Guid.Empty,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(user, Password);
        await userManager.AddToRoleAsync(user, ApplicationRoles.SuperAdmin);

        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password });
        login.EnsureSuccessStatusCode();
        var token = await login.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);
        return client;
    }

    private async Task<(Guid TenantId, HttpClient AdminClient)> OnboardClinicAsync(HttpClient superAdmin)
    {
        var slug = $"bill-{Guid.NewGuid():N}".Substring(0, 16);
        var adminEmail = $"bill-{Guid.NewGuid():N}@clinic.test";

        var onboard = await superAdmin.PostAsJsonAsync("/api/v1/platform/tenants", new
        {
            Slug = slug,
            DisplayName = "Billing Test Clinic",
            AdminEmail = adminEmail,
            AdminPassword = Password,
            HeadquartersCnpj = "11222333000181",
            HeadquartersLegalName = "Billing Matriz",
            PlanCode = "Starter"
        });
        onboard.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await onboard.Content.ReadFromJsonAsync<OnboardTenantResultDto>();
        result.Should().NotBeNull();

        var login = await _factory.CreateClient().PostAsJsonAsync("/api/v1/auth/login", new
        {
            Email = adminEmail,
            Password = Password,
            TenantSlug = slug
        });
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = await login.Content.ReadFromJsonAsync<LoginResponse>();
        var adminClient = _factory.CreateClient();
        adminClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token!.AccessToken);

        return (result!.TenantId, adminClient);
    }

    private sealed record LoginResponse(string AccessToken);
}
