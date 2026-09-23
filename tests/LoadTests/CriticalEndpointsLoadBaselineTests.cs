using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using API.IntegrationTests;
using Core.Application.Authorization;
using Core.Infrastructure.Identity;
using Core.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LoadTests;

/// <summary>
/// In-process load baseline for Fase 10.4 (CI guardrail; staging runbook in docs/arquitetura/load-baseline.md).
/// </summary>
[Collection("IntegrationTests")]
public sealed class CriticalEndpointsLoadBaselineTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const int SamplesPerEndpoint = 20;
    private static readonly TimeSpan P95Budget = TimeSpan.FromMilliseconds(500);

    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>Creates the test fixture.</summary>
    public CriticalEndpointsLoadBaselineTests(WebApplicationFactory<Program> factory) => _factory = factory;

    /// <summary>
    /// Critical read endpoints stay under P95 500 ms on the small CI fixture (Memory cache, SQLite).
    /// </summary>
    [Fact]
    public async Task CriticalEndpoints_P95_UnderFiveHundredMs()
    {
        var client = await CreateStaffClientAsync();
        var paths = new[]
        {
            "/api/v1/intelligence/dashboard",
            "/api/v1/tutors?page=1&pageSize=20",
            "/api/v1/inventory/products?page=1&pageSize=20",
            "/api/v1/financial-titles?page=1&pageSize=20"
        };

        foreach (var path in paths)
        {
            (await client.GetAsync(path)).EnsureSuccessStatusCode();
        }

        var durations = new List<long>();
        foreach (var path in paths)
        {
            durations.AddRange(await SampleEndpointAsync(client, path, SamplesPerEndpoint));
        }

        durations.Sort();
        var p95Index = (int)Math.Ceiling(durations.Count * 0.95) - 1;
        p95Index = Math.Clamp(p95Index, 0, durations.Count - 1);
        var p95 = TimeSpan.FromMilliseconds(durations[p95Index]);

        Assert.True(p95 < P95Budget, $"P95 {p95.TotalMilliseconds:F0} ms exceeded budget {P95Budget.TotalMilliseconds:F0} ms.");
    }

    private static async Task<List<long>> SampleEndpointAsync(HttpClient client, string path, int samples)
    {
        var durations = new List<long>(samples);
        for (var i = 0; i < samples; i++)
        {
            var sw = Stopwatch.StartNew();
            var response = await client.GetAsync(path);
            sw.Stop();
            response.EnsureSuccessStatusCode();
            durations.Add(sw.ElapsedMilliseconds);
        }

        return durations;
    }

    private async Task<HttpClient> CreateStaffClientAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var coreContext = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
        await coreContext.Database.EnsureDeletedAsync();
        await coreContext.Database.MigrateAsync();
        await IntegrationTestDatabaseHelper.MigrateModuleDatabasesAsync(scope);

        await scope.ServiceProvider.GetRequiredService<Core.Application.Common.Interfaces.IAccessProfileSeeder>()
            .EnsureTenantProfilesAsync(IntegrationTestDatabaseHelper.SingleTenantId, CancellationToken.None);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in ApplicationRoles.AllIncludingTutor)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var email = $"load-{Guid.NewGuid():N}@sysvet.com";
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            TenantId = IntegrationTestDatabaseHelper.SingleTenantId
        };
        await userManager.CreateAsync(user, "Password123!");
        await userManager.AddToRoleAsync(user, ApplicationRoles.Admin);

        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = "Password123!" });
        login.EnsureSuccessStatusCode();
        var token = (await login.Content.ReadFromJsonAsync<LoginResponse>())!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private sealed record LoginResponse(string AccessToken, string RefreshToken);
}
