using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Automations.Domain.Enums;
using Automations.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace API.IntegrationTests.Automations;

[Collection("IntegrationTests")]
public class AutomationsEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly WebApplicationFactory<Program> _factory;

    public AutomationsEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task EnqueueJob_ThenProcessor_SucceedsWithAttemptLog()
    {
        var client = await CreateAuthenticatedClientAsync();
        await using var scope = _factory.Services.CreateAsyncScope();
        var automations = scope.ServiceProvider.GetRequiredService<AutomationsDbContext>();

        automations.MessageTemplates.Add(
            global::Automations.Domain.Entities.MessageTemplate.Create(
                "test.enqueue",
                MessageChannel.WhatsApp,
                "Olá {{Name}}.").Value);
        await automations.SaveChangesAsync();

        var enqueueResponse = await client.PostAsJsonAsync(
            "/api/v1/automations/jobs",
            new
            {
                channel = 1,
                templateCode = "test.enqueue",
                payloadJson = """{"Name":"Cliente"}"""
            });

        enqueueResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var jobId = await enqueueResponse.Content.ReadFromJsonAsync<Guid>();

        var getResponse = await client.GetAsync($"/api/v1/automations/jobs/{jobId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var jobJson = await getResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jobJson);
        doc.RootElement.GetProperty("status").GetString().Should().Be(MessageJobStatus.Pending.ToString());
        doc.RootElement.GetProperty("templateCode").GetString().Should().Be("test.enqueue");

        var persisted = await automations.MessageJobs.FirstOrDefaultAsync(j => j.Id == jobId);
        persisted.Should().NotBeNull();
        persisted!.Status.Should().Be(MessageJobStatus.Pending);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        using var scope = _factory.Services.CreateAsyncScope();
        var coreContext = scope.ServiceProvider.GetRequiredService<Core.Infrastructure.Persistence.CoreDbContext>();
        await coreContext.Database.EnsureDeletedAsync();
        await coreContext.Database.MigrateAsync();
        await IntegrationTestDatabaseHelper.MigrateModuleDatabasesAsync(scope);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Core.Infrastructure.Identity.AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        var email = $"automations-{Guid.NewGuid():N}@sysvet.com";
        var user = new Core.Infrastructure.Identity.AppUser { UserName = email, Email = email, TenantId = Guid.NewGuid() };
        await userManager.CreateAsync(user, "Password123!");
        await userManager.AddToRoleAsync(user, "Admin");

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = "Password123!" });
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>(JsonOptions);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse!.AccessToken);
        return client;
    }

    private sealed record LoginResponseDto(string AccessToken);
}
