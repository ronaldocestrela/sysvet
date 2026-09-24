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
using Platform.Infrastructure.Persistence;

namespace LoadTests;

/// <summary>
/// Multi-tenant concurrent guardrail for Fase 10.7 (ADR-060; staging runbook load-capacity.md).
/// </summary>
[Collection("IntegrationTests")]
public sealed class MultiTenantConcurrentLoadTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const int TenantCount = 3;
    private const int ConcurrentRequestsPerTenant = 2;
    private static readonly TimeSpan P95Budget = TimeSpan.FromMilliseconds(500);

    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>Creates the test fixture.</summary>
    public MultiTenantConcurrentLoadTests(WebApplicationFactory<Program> factory) => _factory = factory;

    /// <summary>
    /// Three tenants with two concurrent reads each stay under P95 budget without cross-tenant leakage.
    /// </summary>
    [Fact]
    public async Task MultiTenant_ThreeByTwoConcurrent_P95AndIsolation()
    {
        var clients = await CreateTenantClientsAsync();
        Assert.Equal(TenantCount, clients.Count);
        var foreignSlugs = clients.Select(c => c.Slug).ToList();

        var cpfs = new[] { "52998224725", "10125103360", "12345678909" };
        for (var i = 0; i < clients.Count; i++)
        {
            var (client, slug, _) = clients[i];
            var marker = $"marker-{slug}";
            var create = await client.PostAsJsonAsync("/api/v1/tutors", new
            {
                Id = Guid.NewGuid(),
                Name = marker,
                Email = $"{slug}@load.test",
                Phone = "11999999999",
                Cpf = cpfs[i]
            });
            create.EnsureSuccessStatusCode();
        }

        foreach (var (client, _, _) in clients)
        {
            (await client.GetAsync("/api/v1/tutors?page=1&pageSize=20")).EnsureSuccessStatusCode();
        }

        var tasks = new List<Task<(string Body, long Ms, string OwnSlug)>>(TenantCount * ConcurrentRequestsPerTenant);
        foreach (var (client, slug, _) in clients)
        {
            for (var i = 0; i < ConcurrentRequestsPerTenant; i++)
            {
                tasks.Add(SampleTutorsAsync(client, slug));
            }
        }

        var results = await Task.WhenAll(tasks);
        Assert.All(results, r => Assert.True(r.Ms >= 0));

        var durations = results.Select(r => r.Ms).OrderBy(m => m).ToList();
        var p95Index = Math.Clamp((int)Math.Ceiling(durations.Count * 0.95) - 1, 0, durations.Count - 1);
        var p95 = TimeSpan.FromMilliseconds(durations[p95Index]);
        Assert.True(p95 < P95Budget, $"P95 {p95.TotalMilliseconds:F0} ms exceeded budget.");

        foreach (var (body, _, ownSlug) in results)
        {
            foreach (var other in foreignSlugs.Where(s => !string.Equals(s, ownSlug, StringComparison.Ordinal)))
            {
                Assert.DoesNotContain(other, body, StringComparison.Ordinal);
                Assert.DoesNotContain($"marker-{other}", body, StringComparison.Ordinal);
            }
        }
    }

    private static async Task<(string Body, long Ms, string OwnSlug)> SampleTutorsAsync(HttpClient client, string ownSlug)
    {
        var sw = Stopwatch.StartNew();
        var response = await client.GetAsync("/api/v1/tutors?page=1&pageSize=20");
        sw.Stop();
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        return (body, sw.ElapsedMilliseconds, ownSlug);
    }

    private async Task<List<(HttpClient Client, string Slug, Guid TenantId)>> CreateTenantClientsAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var coreContext = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
        await coreContext.Database.EnsureDeletedAsync();
        await coreContext.Database.MigrateAsync();
        await IntegrationTestDatabaseHelper.MigrateModuleDatabasesAsync(scope);

        var platformContext = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        var subscriptionProvisioner = scope.ServiceProvider.GetRequiredService<Platform.Application.Provisioning.ITenantSubscriptionProvisioner>();
        var tenantProvisioner = scope.ServiceProvider.GetRequiredService<Platform.Application.Provisioning.ITenantProvisioner>();
        var profileSeeder = scope.ServiceProvider.GetRequiredService<Core.Application.Common.Interfaces.IAccessProfileSeeder>();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in ApplicationRoles.AllIncludingTutor)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var clients = new List<(HttpClient, string, Guid)>();

        for (var i = 0; i < TenantCount; i++)
        {
            var tenantId = Guid.NewGuid();
            var slug = $"load-{i}-{tenantId:N}".Substring(0, 20);
            var tenant = global::Platform.Domain.Entities.Tenant.Create(tenantId, slug, $"Load Tenant {i}").Value;
            platformContext.Tenants.Add(tenant);
            await platformContext.SaveChangesAsync();
            Assert.True((await tenantProvisioner.ProvisionAsync(tenantId, tenant.SchemaName, CancellationToken.None)).IsSuccess);
            await subscriptionProvisioner.ProvisionGrandfatherAsync(tenantId);
            await profileSeeder.EnsureTenantProfilesAsync(tenantId, CancellationToken.None);

            var email = $"admin-{slug}@sysvet.com";
            var user = new AppUser
            {
                UserName = email,
                Email = email,
                TenantId = tenantId
            };
            await userManager.CreateAsync(user, "Password123!");
            await userManager.AddToRoleAsync(user, ApplicationRoles.Admin);

            var client = _factory.CreateClient();
            var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = "Password123!" });
            login.EnsureSuccessStatusCode();
            var token = (await login.Content.ReadFromJsonAsync<LoginResponse>())!.AccessToken;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            clients.Add((client, slug, tenantId));
        }

        return clients;
    }

    private sealed record LoginResponse(string AccessToken, string RefreshToken);
}
