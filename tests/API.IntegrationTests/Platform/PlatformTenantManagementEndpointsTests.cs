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
using Platform.Application.Tenants.Dtos;
using Platform.Domain.Entities;
using Xunit;

namespace API.IntegrationTests.Platform;

[Collection("IntegrationTests")]
public class PlatformTenantManagementEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Password = "Password123!";

    private readonly WebApplicationFactory<Program> _factory;

    public PlatformTenantManagementEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task TenantOnboarding_AdminCanAuthenticate_WhenProvisioned()
    {
        var superAdmin = await CreateSuperAdminClientAsync();
        var slug = $"clinica-{Guid.NewGuid():N}".Substring(0, 20);
        var adminEmail = $"admin-{Guid.NewGuid():N}@clinic.test";

        var onboard = await superAdmin.PostAsJsonAsync("/api/v1/platform/tenants", new
        {
            Slug = slug,
            DisplayName = "Clínica Onboard Test",
            AdminEmail = adminEmail,
            AdminPassword = Password,
            HeadquartersCnpj = "11222333000181",
            HeadquartersLegalName = "Matriz Onboard LTDA"
        });

        var onboardBody = await onboard.Content.ReadAsStringAsync();
        onboard.StatusCode.Should().Be(HttpStatusCode.OK, onboardBody);
        var created = await onboard.Content.ReadFromJsonAsync<OnboardTenantResultDto>();
        created.Should().NotBeNull();
        created!.Slug.Should().Be(slug);

        var loginClient = _factory.CreateClient();
        var login = await loginClient.PostAsJsonAsync("/api/v1/auth/login", new { Email = adminEmail, Password });
        login.StatusCode.Should().Be(HttpStatusCode.OK);

        var token = await login.Content.ReadFromJsonAsync<LoginResponse>();
        var clinicClient = _factory.CreateClient();
        clinicClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);

        var me = await clinicClient.GetAsync("/api/v1/auth/me");
        me.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SuspendedTenant_ForbiddenOnClinicApi()
    {
        var superAdmin = await CreateSuperAdminClientAsync();
        var slug = $"suspend-{Guid.NewGuid():N}".Substring(0, 18);
        var adminEmail = $"suspend-{Guid.NewGuid():N}@clinic.test";

        var onboard = await superAdmin.PostAsJsonAsync("/api/v1/platform/tenants", new
        {
            Slug = slug,
            DisplayName = "Clínica Suspend",
            AdminEmail = adminEmail,
            AdminPassword = Password,
            HeadquartersCnpj = "11222333000181",
            HeadquartersLegalName = "Matriz Suspend"
        });
        onboard.EnsureSuccessStatusCode();
        var created = (await onboard.Content.ReadFromJsonAsync<OnboardTenantResultDto>())!;

        var suspend = await superAdmin.PatchAsJsonAsync(
            $"/api/v1/platform/tenants/{created.TenantId}/status",
            new { Status = TenantStatus.Suspended });
        suspend.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var login = await _factory.CreateClient().PostAsJsonAsync("/api/v1/auth/login", new { Email = adminEmail, Password });
        login.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Branch_LinkedToHeadquarters_WhenAdded()
    {
        var superAdmin = await CreateSuperAdminClientAsync();
        var slug = $"branch-{Guid.NewGuid():N}".Substring(0, 18);
        var adminEmail = $"branch-{Guid.NewGuid():N}@clinic.test";

        var onboard = await superAdmin.PostAsJsonAsync("/api/v1/platform/tenants", new
        {
            Slug = slug,
            DisplayName = "Clínica Filiais",
            AdminEmail = adminEmail,
            AdminPassword = Password,
            HeadquartersCnpj = "11222333000181",
            HeadquartersLegalName = "Matriz Filiais"
        });
        onboard.EnsureSuccessStatusCode();
        var created = (await onboard.Content.ReadFromJsonAsync<OnboardTenantResultDto>())!;

        var addBranch = await superAdmin.PostAsJsonAsync($"/api/v1/platform/tenants/{created.TenantId}/branches", new
        {
            Cnpj = "12345678000195",
            LegalName = "Filial SP",
            IsHeadquarters = false
        });
        addBranch.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await superAdmin.GetFromJsonAsync<List<BranchDto>>($"/api/v1/platform/tenants/{created.TenantId}/branches");
        list.Should().NotBeNull();
        list!.Should().HaveCount(2);
        list.Should().Contain(b => b.IsHeadquarters && b.Cnpj == "11222333000181");
        list.Should().Contain(b => !b.IsHeadquarters && b.Cnpj == "12345678000195");
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
        const string email = "superadmin-test@vetnexus.app";
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
}
