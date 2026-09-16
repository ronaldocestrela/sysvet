using System.Net;
using System.Net.Http.Json;
using Core.Application.Authorization;
using Core.Application.Preferences.Commands;
using Core.Domain.Authorization;
using Core.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace API.IntegrationTests;

[Collection("IntegrationTests")]
public class UserAccessProfileEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public UserAccessProfileEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateClientForRoleAsync(string role)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Core.Infrastructure.Persistence.CoreDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var tenantId = Guid.NewGuid();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var r in ApplicationRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(r))
            {
                await roleManager.CreateAsync(new IdentityRole(r));
            }
        }

        var user = new AppUser
        {
            UserName = $"{role}@sysvet.com",
            Email = $"{role}@sysvet.com",
            TenantId = tenantId
        };
        await userManager.CreateAsync(user, "Password123!");
        await userManager.AddToRoleAsync(user, role);

        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = user.Email, Password = "Password123!" });
        var tokens = await login.Content.ReadFromJsonAsync<AuthTokensResponse>();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens!.AccessToken);
        return client;
    }

    [Fact]
    public async Task GetMe_Cashier_ShouldNotIncludeTutorsMenu()
    {
        var client = await CreateClientForRoleAsync(ApplicationRoles.Cashier);

        var response = await client.GetAsync("/api/v1/auth/me");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var me = await response.Content.ReadFromJsonAsync<MeResponseExtended>();
        me!.Menus.Should().NotContain("tutors");
    }

    [Fact]
    public async Task GetUsers_AsCashier_ShouldReturnForbidden()
    {
        var client = await CreateClientForRoleAsync(ApplicationRoles.Cashier);

        var response = await client.GetAsync("/api/v1/users?page=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteTutor_AsReceptionist_ShouldReturnForbidden()
    {
        var client = await CreateClientForRoleAsync(ApplicationRoles.Receptionist);

        var create = new Core.Application.Tutors.Commands.CreateTutorCommand(
            Guid.NewGuid(), "Del Test", "del@test.com", "63683891416", "11999999999");
        var createResponse = await client.PostAsJsonAsync("/api/v1/tutors", create);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var tutorId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var deleteResponse = await client.DeleteAsync($"/api/v1/tutors/{tutorId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPreferences_WhenEmpty_ShouldReturnDefaults()
    {
        var client = await CreateClientForRoleAsync(ApplicationRoles.Admin);

        var get = await client.GetAsync("/api/v1/me/preferences");
        get.StatusCode.Should().Be(HttpStatusCode.OK);
        var prefs = await get.Content.ReadFromJsonAsync<PreferencesResponse>();
        prefs!.ShortcutsJson.Should().Be("{}");
    }

    private sealed class MeResponseExtended
    {
        public IReadOnlyList<string> Menus { get; set; } = [];
    }

    private sealed class PreferencesResponse
    {
        public string ShortcutsJson { get; set; } = string.Empty;
    }
}
