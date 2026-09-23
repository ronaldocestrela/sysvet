using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Core.Application.Authorization;
using Core.Application.Common.Interfaces;
using Core.Infrastructure.Identity;
using Core.Infrastructure.Persistence;
using FluentAssertions;
using Intelligence.Application.Dashboard.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace API.IntegrationTests.Intelligence;

[Collection("IntegrationTests")]
public class IntelligenceEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Password = "Password123!";

    private readonly WebApplicationFactory<Program> _factory;

    public IntelligenceEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Dashboard_ReturnsTodayWidgets_ForAdmin()
    {
        var client = await CreateStaffClientAsync(ApplicationRoles.Admin);
        var response = await client.GetAsync("/api/v1/intelligence/dashboard");
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);

        var dashboard = await response.Content.ReadFromJsonAsync<TenantDashboardDto>();
        dashboard.Should().NotBeNull();
        dashboard!.Widgets.Should().NotBeEmpty();
        dashboard.Widgets.Should().Contain(w => w.Key == "SalesToday");
    }

    [Fact]
    public async Task AbcCustomersReport_ReturnsOk()
    {
        var client = await CreateStaffClientAsync(ApplicationRoles.Admin);
        var from = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
        var to = DateOnly.FromDateTime(DateTime.UtcNow);
        var response = await client.GetAsync(
            $"/api/v1/intelligence/reports/abc-customers?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}");
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);
    }

    [Fact]
    public async Task AbcCustomersExport_ReturnsCsv()
    {
        var client = await CreateStaffClientAsync(ApplicationRoles.Admin);
        var from = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
        var to = DateOnly.FromDateTime(DateTime.UtcNow);
        var response = await client.GetAsync(
            $"/api/v1/intelligence/reports/abc-customers/export?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}");
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/csv");
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Dashboard_LoadsTodayKpis_UnderThreeSeconds()
    {
        var client = await CreateStaffClientAsync(ApplicationRoles.Admin);
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var response = await client.GetAsync("/api/v1/intelligence/dashboard");
        sw.Stop();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(3));
    }

    private async Task<HttpClient> CreateStaffClientAsync(string role)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        await PrepareDatabasesAsync(scope);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var r in ApplicationRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(r))
            {
                await roleManager.CreateAsync(new IdentityRole(r));
            }
        }

        var email = $"intel-{Guid.NewGuid():N}@sysvet.com";
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            TenantId = IntegrationTestDatabaseHelper.SingleTenantId
        };
        await userManager.CreateAsync(user, Password);
        await userManager.AddToRoleAsync(user, role);

        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password });
        var token = await login.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);
        return client;
    }

    private static async Task PrepareDatabasesAsync(IServiceScope scope)
    {
        var coreContext = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
        await coreContext.Database.EnsureDeletedAsync();
        await coreContext.Database.MigrateAsync();
        await IntegrationTestDatabaseHelper.MigrateModuleDatabasesAsync(scope);

        await scope.ServiceProvider.GetRequiredService<IAccessProfileSeeder>()
            .EnsureTenantProfilesAsync(IntegrationTestDatabaseHelper.SingleTenantId, CancellationToken.None);

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in ApplicationRoles.AllIncludingTutor)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private sealed record LoginResponse(string AccessToken, string RefreshToken);
}
