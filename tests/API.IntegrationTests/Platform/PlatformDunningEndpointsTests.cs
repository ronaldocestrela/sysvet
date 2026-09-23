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
using Platform.Application.Dunning;
using Platform.Application.Tenants.Dtos;
using Platform.Domain.Entities;
using Platform.Infrastructure.Persistence;
using Xunit;

namespace API.IntegrationTests.Platform;

[Collection("IntegrationTests")]
public class PlatformDunningEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Password = "Password123!";
    private const string ValidCpf = "39053344705";

    private readonly WebApplicationFactory<Program> _factory;

    public PlatformDunningEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task SimulatedDelinquency_SuspendsOperationalAccess()
    {
        var superAdmin = await CreateSuperAdminClientAsync();
        var (tenantId, adminClient) = await OnboardClinicAsync(superAdmin);
        await SetupBillingProfileAsync(superAdmin, tenantId);
        await SetSubscriptionPeriodDueAsync(tenantId);

        var charge = await superAdmin.PostAsync($"/api/v1/platform/tenants/{tenantId}/billing/charge", null);
        charge.EnsureSuccessStatusCode();
        var chargeBody = await charge.Content.ReadFromJsonAsync<ChargeTenantBillingResultDto>();

        await SimulatePastDueAndLockAsync(tenantId, chargeBody!.InvoiceId);

        var blocked = await adminClient.GetAsync("/api/v1/tutors");
        blocked.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var standing = await adminClient.GetAsync("/api/v1/billing/standing");
        standing.StatusCode.Should().Be(HttpStatusCode.OK);
        var standingBody = await standing.Content.ReadFromJsonAsync<ClinicBillingStandingDto>();
        standingBody!.IsOperationallyLocked.Should().BeTrue();

        using var webhookClient = _factory.CreateClient();
        var webhook = await webhookClient.PostAsJsonAsync("/api/v1/platform/webhooks/asaas", new
        {
            @event = "PAYMENT_CONFIRMED",
            payment = new
            {
                id = chargeBody.GatewayPaymentId,
                externalReference = chargeBody.InvoiceId.ToString()
            }
        });
        webhook.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var restored = await adminClient.GetAsync("/api/v1/tutors");
        restored.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task SimulatePastDueAndLockAsync(Guid tenantId, Guid invoiceId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var platform = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        var pastDueSince = DateTimeOffset.UtcNow.AddDays(-10);
        var subscription = await platform.TenantSubscriptions.FirstAsync(s => s.TenantId == tenantId);
        subscription.RecordPaymentOverdue(pastDueSince);
        subscription.EvaluateOperationalLock(DateTimeOffset.UtcNow, lockAfterDays: 7);

        var invoice = await platform.BillingInvoices.FirstAsync(i => i.Id == invoiceId);
        invoice.MarkFailed();

        await platform.SaveChangesAsync();
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
        const string email = "superadmin-dunning@vetnexus.app";
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
        var slug = $"dun-{Guid.NewGuid():N}".Substring(0, 16);
        var adminEmail = $"dun-{Guid.NewGuid():N}@clinic.test";

        var onboard = await superAdmin.PostAsJsonAsync("/api/v1/platform/tenants", new
        {
            Slug = slug,
            DisplayName = "Dunning Test Clinic",
            AdminEmail = adminEmail,
            AdminPassword = Password,
            HeadquartersCnpj = "11222333000181",
            HeadquartersLegalName = "Dunning Matriz",
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
            new AuthenticationHeaderValue("Bearer", token!.AccessToken);

        return (result!.TenantId, adminClient);
    }

    private sealed record LoginResponse(string AccessToken);
}
