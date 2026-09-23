using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using API.IntegrationTests;
using Core.Application.Authorization;
using Core.Infrastructure.Identity;
using Core.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Metrics;
using Platform.Application.Tenants.Dtos;
using Platform.Domain.Entities;
using Platform.Domain.Services;
using Platform.Infrastructure.Persistence;
using Xunit;

namespace API.IntegrationTests.Platform;

[Collection("IntegrationTests")]
public class PlatformSaasMetricsEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Password = "Password123!";

    private readonly WebApplicationFactory<Program> _factory;

    public PlatformSaasMetricsEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task SaasMetrics_MatchBilling_WithinDocumentedTolerance()
    {
        var superAdmin = await CreateSuperAdminClientAsync();
        var periodStart = new DateTimeOffset(2026, 3, 10, 12, 0, 0, TimeSpan.Zero);
        var paidAt = new DateTimeOffset(2026, 3, 12, 12, 0, 0, TimeSpan.Zero);

        await SeedInvoiceAsync(superAdmin, periodStart, paidAt, 199m, BillingInvoiceStatus.Paid);

        var response = await superAdmin.GetAsync("/api/v1/platform/metrics?year=2026&month=3");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var metrics = await response.Content.ReadFromJsonAsync<PlatformSaasMetricsDto>();
        metrics.Should().NotBeNull();
        metrics!.BilledMrr.Should().BeApproximately(199m, SaasMetricsCalculator.MoneyTolerance);
        metrics.Arr.Should().BeApproximately(199m * 12, SaasMetricsCalculator.MoneyTolerance);
        metrics.CashIn.Should().BeApproximately(199m, SaasMetricsCalculator.MoneyTolerance);
    }

    [Fact]
    public async Task SaasMetrics_Forbidden_WhenNotSuperAdmin()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var coreContext = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
        await coreContext.Database.EnsureDeletedAsync();
        await coreContext.Database.MigrateAsync();
        await IntegrationTestDatabaseHelper.MigrateModuleDatabasesAsync(scope);

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync(ApplicationRoles.Admin))
        {
            await roleManager.CreateAsync(new IdentityRole(ApplicationRoles.Admin));
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var email = $"clinic-admin-{Guid.NewGuid():N}@test.com";
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            TenantId = Guid.NewGuid(),
            EmailConfirmed = true
        };
        await userManager.CreateAsync(user, Password);
        await userManager.AddToRoleAsync(user, ApplicationRoles.Admin);

        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password });
        login.EnsureSuccessStatusCode();
        var token = await login.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);

        var response = await client.GetAsync("/api/v1/platform/metrics?year=2026&month=3");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task SeedInvoiceAsync(
        HttpClient superAdmin,
        DateTimeOffset periodStart,
        DateTimeOffset? paidAt,
        decimal amount,
        BillingInvoiceStatus status)
    {
        var slug = $"m{Guid.NewGuid():N}"[..16];
        var onboard = await superAdmin.PostAsJsonAsync("/api/v1/platform/tenants", new
        {
            Slug = slug,
            DisplayName = "Metrics Tenant",
            AdminEmail = $"metrics-{Guid.NewGuid():N}@clinic.test",
            AdminPassword = Password,
            HeadquartersCnpj = "11222333000181",
            HeadquartersLegalName = "Metrics Matriz",
            PlanCode = "Starter"
        });
        onboard.EnsureSuccessStatusCode();
        var onboardBody = await onboard.Content.ReadFromJsonAsync<OnboardTenantResultDto>();
        var tenantId = onboardBody!.TenantId;

        await using var scope = _factory.Services.CreateAsyncScope();
        var platform = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        var invoice = BillingInvoice.Open(tenantId, periodStart, periodStart.AddDays(30), amount).Value;
        if (status == BillingInvoiceStatus.Paid && paidAt is not null)
        {
            invoice.MarkPaid(paidAt.Value);
        }

        await platform.BillingInvoices.AddAsync(invoice);
        await platform.SaveChangesAsync();
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
        const string email = "superadmin-metrics@vetnexus.app";
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

    private sealed record LoginResponse(string AccessToken);
}
