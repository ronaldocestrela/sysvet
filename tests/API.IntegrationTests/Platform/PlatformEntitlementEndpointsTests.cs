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
using Platform.Infrastructure.Persistence;
using Platform.Application.Subscriptions;
using Platform.Application.Tenants.Dtos;
using Platform.Domain.Entities;
using Core.Domain.Entitlements;
using Xunit;

namespace API.IntegrationTests.Platform;

[Collection("IntegrationTests")]
public class PlatformEntitlementEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Password = "Password123!";

    private readonly WebApplicationFactory<Program> _factory;

    public PlatformEntitlementEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task FeatureFlag_DisablesModule_OnApiAndMenus()
    {
        var superAdmin = await CreateSuperAdminClientAsync();
        var (tenantId, adminClient) = await OnboardClinicWithFiscalAsync(superAdmin);

        var entitlementsBefore = await superAdmin.GetFromJsonAsync<TenantEntitlementsDto>(
            $"/api/v1/platform/tenants/{tenantId}/entitlements");
        entitlementsBefore!.Modules.Should().Contain(CommercialModule.Fiscal);

        var disable = await superAdmin.PutAsJsonAsync(
            $"/api/v1/platform/tenants/{tenantId}/flags/{CommercialModule.Fiscal}",
            new { State = FeatureFlagState.Disabled });
        disable.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var entitlementsAfter = await superAdmin.GetFromJsonAsync<TenantEntitlementsDto>(
            $"/api/v1/platform/tenants/{tenantId}/entitlements");
        entitlementsAfter!.Modules.Should().NotContain(CommercialModule.Fiscal);

        var fiscal = await adminClient.GetAsync("/api/v1/fiscal/planning?from=2026-01-01&to=2026-01-31");
        fiscal.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var me = await adminClient.GetFromJsonAsync<MeResponse>("/api/v1/auth/me");
        me!.Menus.Should().NotContain("fiscal");

        var enable = await superAdmin.PutAsJsonAsync(
            $"/api/v1/platform/tenants/{tenantId}/flags/{CommercialModule.Fiscal}",
            new { State = FeatureFlagState.Enabled });
        enable.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        fiscal = await adminClient.GetAsync("/api/v1/fiscal/planning?from=2026-01-01&to=2026-01-31");
        fiscal.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);

        var meAfter = await adminClient.GetFromJsonAsync<MeResponse>("/api/v1/auth/me");
        meAfter!.Menus.Should().Contain("fiscal");
    }

    [Fact]
    public async Task PlanChange_ReturnsProration_WhenMidCycle()
    {
        var superAdmin = await CreateSuperAdminClientAsync();
        var (tenantId, _) = await OnboardClinicAsync(superAdmin, "Starter");

        var change = await superAdmin.PostAsJsonAsync(
            $"/api/v1/platform/tenants/{tenantId}/subscription/change",
            new { PlanCode = "Pro" });
        change.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await change.Content.ReadFromJsonAsync<PlanChangeResultDto>();
        body.Should().NotBeNull();
        body!.NewPlanId.Should().NotBeEmpty();
    }

    private async Task<(Guid TenantId, HttpClient AdminClient)> OnboardClinicWithFiscalAsync(HttpClient superAdmin)
    {
        var (tenantId, adminClient) = await OnboardClinicAsync(superAdmin, "Hospital24h");
        var activate = await superAdmin.PostAsync($"/api/v1/platform/tenants/{tenantId}/addons/Fiscal/activate", null);
        activate.StatusCode.Should().Be(HttpStatusCode.OK);
        return (tenantId, adminClient);
    }

    private async Task<(Guid TenantId, HttpClient AdminClient)> OnboardClinicAsync(HttpClient superAdmin, string planCode)
    {
        var slug = $"ent-{Guid.NewGuid():N}".Substring(0, 16);
        var adminEmail = $"ent-{Guid.NewGuid():N}@clinic.test";

        var onboard = await superAdmin.PostAsJsonAsync("/api/v1/platform/tenants", new
        {
            Slug = slug,
            DisplayName = "Entitlement Test Clinic",
            AdminEmail = adminEmail,
            AdminPassword = Password,
            HeadquartersCnpj = "11222333000181",
            HeadquartersLegalName = "Entitlement Matriz",
            PlanCode = planCode
        });
        onboard.EnsureSuccessStatusCode();
        var created = (await onboard.Content.ReadFromJsonAsync<OnboardTenantResultDto>())!;

        var loginClient = _factory.CreateClient();
        var login = await loginClient.PostAsJsonAsync("/api/v1/auth/login", new { Email = adminEmail, Password });
        login.EnsureSuccessStatusCode();
        var token = (await login.Content.ReadFromJsonAsync<LoginResponse>())!.AccessToken;
        var adminClient = _factory.CreateClient();
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return (created.TenantId, adminClient);
    }

    private async Task<HttpClient> CreateSuperAdminClientAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var coreContext = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
        await coreContext.Database.EnsureDeletedAsync();
        await coreContext.Database.MigrateAsync();
        await IntegrationTestDatabaseHelper.MigrateModuleDatabasesAsync(scope);

        var platformContext = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        await platformContext.Database.MigrateAsync();
        await PlatformCatalogTestSeeder.EnsureCatalogAsync(scope.ServiceProvider);

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in ApplicationRoles.AllIncludingTutor)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        const string email = "superadmin-ent@vetnexus.app";
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

    private sealed record LoginResponse(string AccessToken, string RefreshToken);

    private sealed record MeResponse(IReadOnlyList<string> Menus);
}
