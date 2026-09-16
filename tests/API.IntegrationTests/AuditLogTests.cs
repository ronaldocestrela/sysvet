using System.Net;
using System.Net.Http.Json;
using Core.Application.AuditLogs.Queries;
using Core.Application.Common;
using Core.Application.Tutors.Commands;
using Core.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace API.IntegrationTests;

[Collection("IntegrationTests")]
public class AuditLogTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuditLogTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string roleName)
    {
        using var scope = _factory.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<Core.Infrastructure.Persistence.CoreDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync(roleName))
            await roleManager.CreateAsync(new IdentityRole(roleName));

        var user = await userManager.FindByEmailAsync($"{roleName}@sysvet.com");
        if (user == null)
        {
            user = new AppUser { UserName = $"{roleName}@sysvet.com", Email = $"{roleName}@sysvet.com", TenantId = Guid.NewGuid() };
            await userManager.CreateAsync(user, "Password123!");
            await userManager.AddToRoleAsync(user, roleName);
        }

        var client = _factory.CreateClient();

        var loginRequest = new { Email = $"{roleName}@sysvet.com", Password = "Password123!" };
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        var loginContent = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginContent!.AccessToken);
        return client;
    }

    [Fact]
    public async Task AuditLog_IsCreated_WhenEntityIsAdded()
    {
        var client = await CreateAuthenticatedClientAsync("Admin");
        var command = new CreateTutorCommand(Guid.NewGuid(), "John Doe", "john@example.com", "63683891416", "11999999999");

        var response = await client.PostAsJsonAsync("/api/v1/tutors", command);

        var content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, content);

        var listResponse = await client.GetAsync("/api/v1/audit-logs?entityName=Tutor");
        var listBody = await listResponse.Content.ReadAsStringAsync();
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK, listBody);

        var page = await listResponse.Content.ReadFromJsonAsync<PagedResult<AuditLogDto>>();
        page!.Items.Should().Contain(l => l.EntityName == "Tutor" && l.Action == "Added");
        page.Items.Should().OnlyContain(l => l.UserId != Guid.Empty);
    }

    [Fact]
    public async Task AuditLog_IsQueryable_WhenTutorIsUpdated()
    {
        var client = await CreateAuthenticatedClientAsync("Admin");
        var tutorId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/v1/tutors", new CreateTutorCommand(tutorId, "John Doe", "john@example.com", "63683891416", "11999999999"));

        var update = new UpdateTutorCommand(tutorId, "John Smith", "smith@example.com", "11888888888");
        var updateResponse = await client.PutAsJsonAsync($"/api/v1/tutors/{tutorId}", update);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listResponse = await client.GetAsync($"/api/v1/audit-logs?entityName=Tutor&entityId={tutorId}");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var page = await listResponse.Content.ReadFromJsonAsync<PagedResult<AuditLogDto>>();
        page!.Items.Should().Contain(l => l.Action == "Modified");
    }

    [Fact]
    public async Task ListAuditLogs_AsCashier_ReturnsForbidden()
    {
        var client = await CreateAuthenticatedClientAsync("Cashier");

        var response = await client.GetAsync("/api/v1/audit-logs");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
