using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Core.Application.Authorization;
using Core.Infrastructure.Identity;
using Core.Infrastructure.Persistence;
using API.IntegrationTests;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Billing;
using Platform.Application.Impersonation;
using Platform.Application.Tenants.Dtos;
using Platform.Domain.Entities;
using Platform.Infrastructure.Persistence;
using Xunit;

namespace API.IntegrationTests.Platform;

[Collection("IntegrationTests")]
public class PlatformNfseAndImpersonationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Password = "Password123!";
    private const string ValidCpf = "39053344705";

    private readonly WebApplicationFactory<Program> _factory;

    public PlatformNfseAndImpersonationTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task SaasNfse_Issued_WhenInvoiceSettled()
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
        await webhookClient.PostAsJsonAsync("/api/v1/platform/webhooks/asaas", new
        {
            @event = "PAYMENT_CONFIRMED",
            payment = new { id = paymentId, externalReference = invoiceId.ToString() }
        });

        await using var scope = _factory.Services.CreateAsyncScope();
        var platform = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        var nfse = await platform.SaasServiceInvoices.FirstOrDefaultAsync(i => i.BillingInvoiceId == invoiceId);
        nfse.Should().NotBeNull();
        nfse!.Status.Should().Be(SaasServiceInvoiceStatus.Authorized);
        nfse.Amount.Should().BeGreaterThan(0);

        var count = await platform.SaasServiceInvoices.CountAsync(i => i.BillingInvoiceId == invoiceId);
        count.Should().Be(1);
    }

    [Fact]
    public async Task Impersonation_WritesImmutableAudit_AndSessionExpires()
    {
        var superAdmin = await CreateSuperAdminClientAsync();
        var (tenantId, _) = await OnboardClinicAsync(superAdmin);

        var start = await superAdmin.PostAsync($"/api/v1/platform/tenants/{tenantId}/impersonation", null);
        start.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await start.Content.ReadFromJsonAsync<StartImpersonationResultDto>();
        body.Should().NotBeNull();

        await using var scope = _factory.Services.CreateAsyncScope();
        var platform = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        var audits = await platform.ImpersonationAuditEntries.Where(a => a.SessionId == body!.SessionId).ToListAsync();
        audits.Should().ContainSingle(a => a.Action == "Started");
        audits[0].ClientIp.Should().NotBeNullOrWhiteSpace();

        using var impersonationClient = _factory.CreateClient();
        impersonationClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", body!.AccessToken);

        var tutors = await impersonationClient.GetAsync("/api/v1/tutors?page=1&pageSize=1");
        tutors.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden);

        var end = await superAdmin.PostAsync($"/api/v1/platform/impersonation/{body.SessionId}/end", null);
        end.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var blocked = await impersonationClient.GetAsync("/api/v1/tutors?page=1&pageSize=1");
        blocked.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        audits = await platform.ImpersonationAuditEntries.Where(a => a.SessionId == body.SessionId).ToListAsync();
        audits.Should().Contain(a => a.Action == "Ended");
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
        const string email = "superadmin-nfse@vetnexus.app";
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
        var slug = $"nfse-{Guid.NewGuid():N}"[..16];
        var adminEmail = $"nfse-{Guid.NewGuid():N}@clinic.test";

        var onboard = await superAdmin.PostAsJsonAsync("/api/v1/platform/tenants", new
        {
            Slug = slug,
            DisplayName = "NFS-e Test Clinic",
            AdminEmail = adminEmail,
            AdminPassword = Password,
            HeadquartersCnpj = "11222333000181",
            HeadquartersLegalName = "NFS-e Matriz",
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
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);

        return (result!.TenantId, adminClient);
    }

    private async Task SetupBillingProfileAsync(HttpClient superAdmin, Guid tenantId)
    {
        await superAdmin.PutAsJsonAsync(
            $"/api/v1/platform/tenants/{tenantId}/billing/customer",
            new { Name = "Clínica Teste", Email = "billing@test.com", CpfCnpj = ValidCpf });
        await superAdmin.PutAsJsonAsync(
            $"/api/v1/platform/tenants/{tenantId}/billing/payment-method",
            new { Kind = BillingPaymentMethodKind.Pix, CreditCardToken = (string?)null });
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

    private sealed record LoginResponse(string AccessToken);
}
