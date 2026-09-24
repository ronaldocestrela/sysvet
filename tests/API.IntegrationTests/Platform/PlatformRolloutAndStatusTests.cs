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
using Platform.Application.Status;
using Platform.Application.Tenants.Dtos;
using Platform.Domain.Entities;

namespace API.IntegrationTests.Platform;

[Collection("IntegrationTests")]
public sealed class PlatformRolloutAndStatusTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Password = "Password123!";

    private readonly WebApplicationFactory<Program> _factory;

    public PlatformRolloutAndStatusTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task SetReleaseRing_UpdatesTenantDetail()
    {
        var superAdmin = await CreateSuperAdminClientAsync();
        var tenantId = IntegrationTestDatabaseHelper.SingleTenantId;

        var patch = await superAdmin.PatchAsJsonAsync(
            $"/api/v1/platform/tenants/{tenantId}/release-ring",
            new { Ring = ReleaseRing.Canary });
        patch.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var detail = await superAdmin.GetFromJsonAsync<TenantDetailDto>($"/api/v1/platform/tenants/{tenantId}");
        detail!.ReleaseRing.Should().Be(ReleaseRing.Canary);
    }

    [Fact]
    public async Task PublicStatus_ReflectsOpenIncident()
    {
        var superAdmin = await CreateSuperAdminClientAsync();
        var create = await superAdmin.PostAsJsonAsync("/api/v1/platform/status/incidents", new
        {
            Title = "Sync delays",
            Impact = StatusIncidentImpact.Minor,
            Components = "sync"
        });
        create.EnsureSuccessStatusCode();
        var incident = await create.Content.ReadFromJsonAsync<StatusIncidentDto>();

        var anonymous = _factory.CreateClient();
        var status = await anonymous.GetFromJsonAsync<PublicStatusDto>("/api/v1/public/status");
        status.Should().NotBeNull();
        status!.OpenIncidents.Should().Contain(i => i.Id == incident!.Id);

        var resolve = await superAdmin.PatchAsJsonAsync(
            $"/api/v1/platform/status/incidents/{incident!.Id}/resolve",
            new { });
        resolve.StatusCode.Should().Be(HttpStatusCode.OK); // Resolve returns incident body

        status = await anonymous.GetFromJsonAsync<PublicStatusDto>("/api/v1/public/status");
        status!.OpenIncidents.Should().NotContain(i => i.Id == incident.Id);
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
        const string email = "superadmin-rollout@vetnexus.app";
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
