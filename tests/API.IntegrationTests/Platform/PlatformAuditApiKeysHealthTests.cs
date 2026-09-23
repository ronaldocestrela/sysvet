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
using Platform.Application.ApiKeys;
using Platform.Application.Auditing;
using Platform.Application.Health;
using Platform.Application.Tenants.Dtos;
using Platform.Infrastructure.Persistence;
using Xunit;

namespace API.IntegrationTests.Platform;

[Collection("IntegrationTests")]
public class PlatformAuditApiKeysHealthTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Password = "Password123!";

    private readonly WebApplicationFactory<Program> _factory;

    public PlatformAuditApiKeysHealthTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task RevokedApiKey_FailsImmediately()
    {
        var superAdmin = await CreateSuperAdminClientAsync();
        var (tenantId, _) = await OnboardClinicAsync(superAdmin, "audit-a");

        var create = await superAdmin.PostAsJsonAsync(
            $"/api/v1/platform/tenants/{tenantId}/api-keys",
            new { PartnerName = "Contabilidade Teste" });
        create.StatusCode.Should().Be(HttpStatusCode.OK);
        var keyBody = await create.Content.ReadFromJsonAsync<CreatePartnerApiKeyResultDto>();
        keyBody.Should().NotBeNull();

        using var partner = _factory.CreateClient();
        partner.DefaultRequestHeaders.Add("X-Api-Key", keyBody!.Secret);
        var ok = await partner.GetAsync("/api/v1/partner/health");
        ok.StatusCode.Should().Be(HttpStatusCode.OK);

        var revoke = await superAdmin.PostAsync(
            $"/api/v1/platform/tenants/{tenantId}/api-keys/{keyBody.KeyId}/revoke",
            null);
        revoke.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var blocked = await partner.GetAsync("/api/v1/partner/health");
        blocked.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TenantHealth_Visible_PerTenant()
    {
        var superAdmin = await CreateSuperAdminClientAsync();
        var (tenantA, adminA) = await OnboardClinicAsync(superAdmin, "health-a");
        var (tenantB, _) = await OnboardClinicAsync(superAdmin, "health-b");

        var me = await adminA.GetAsync("/api/v1/auth/me");
        me.StatusCode.Should().Be(HttpStatusCode.OK);

        var healthA = await superAdmin.GetAsync($"/api/v1/platform/tenants/{tenantA}/health");
        healthA.StatusCode.Should().Be(HttpStatusCode.OK);
        var bodyA = await healthA.Content.ReadFromJsonAsync<TenantHealthDto>();
        bodyA.Should().NotBeNull();
        bodyA!.TenantId.Should().Be(tenantA);
        bodyA.RequestsToday.Should().BeGreaterThan(0);

        var healthB = await superAdmin.GetAsync($"/api/v1/platform/tenants/{tenantB}/health");
        var bodyB = await healthB.Content.ReadFromJsonAsync<TenantHealthDto>();
        bodyB.Should().NotBeNull();
        bodyA!.RequestsToday.Should().BeGreaterThan(bodyB!.RequestsToday);
    }

    [Fact]
    public async Task LoginAttempt_WritesPlatformLoginLog_WithIpAndUserAgent()
    {
        var superAdmin = await CreateSuperAdminClientAsync();
        var (_, adminClient) = await OnboardClinicAsync(superAdmin, "login-log");
        var me = await adminClient.GetAsync("/api/v1/auth/me");
        me.StatusCode.Should().Be(HttpStatusCode.OK);

        var logs = await superAdmin.GetAsync("/api/v1/platform/login-logs?take=20");
        logs.StatusCode.Should().Be(HttpStatusCode.OK);
        var rows = await logs.Content.ReadFromJsonAsync<List<PlatformLoginLogDto>>();
        rows.Should().NotBeNull();
        rows!.Should().Contain(l => l.Succeeded && !string.IsNullOrWhiteSpace(l.ClientIp));
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
        const string email = "superadmin-audit@vetnexus.app";
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

    private async Task<(Guid TenantId, HttpClient AdminClient)> OnboardClinicAsync(HttpClient superAdmin, string slugPrefix)
    {
        var slug = $"{slugPrefix}-{Guid.NewGuid():N}"[..20];
        var adminEmail = $"{slug}@clinic.test";

        var onboard = await superAdmin.PostAsJsonAsync("/api/v1/platform/tenants", new
        {
            Slug = slug,
            DisplayName = "Audit Test Clinic",
            AdminEmail = adminEmail,
            AdminPassword = Password,
            HeadquartersCnpj = "11222333000181",
            HeadquartersLegalName = "Audit Matriz",
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

    private sealed record LoginResponse(string AccessToken);
}
