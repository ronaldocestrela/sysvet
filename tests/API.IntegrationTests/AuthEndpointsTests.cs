using System.Net;
using System.Net.Http.Json;
using Core.Application.Authorization;
using Core.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace API.IntegrationTests;

[Collection("IntegrationTests")]
public class AuthEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private async Task SeedUserAsync()
    {
        using var scope = _factory.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<Core.Infrastructure.Persistence.CoreDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var roleName in ApplicationRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole(roleName));
        }

        var existing = await userManager.FindByEmailAsync("admin@sysvet.com");
        if (existing is not null)
        {
            await userManager.DeleteAsync(existing);
        }

        var tenantId = Guid.NewGuid();
        var user = new AppUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "admin@sysvet.com",
            Email = "admin@sysvet.com",
            TenantId = tenantId
        };
        await userManager.CreateAsync(user, "Password123!");
        await userManager.AddToRoleAsync(user, "Admin");
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsJwtToken()
    {
        await SeedUserAsync();
        var client = _factory.CreateClient();
        var request = new { Email = "admin@sysvet.com", Password = "Password123!" };

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<AuthTokensResponse>();
        content.Should().NotBeNull();
        content!.AccessToken.Should().NotBeNullOrEmpty();
        content.RefreshToken.Should().NotBeNullOrEmpty();
        content.ExpiresInSeconds.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        await SeedUserAsync();
        var client = _factory.CreateClient();
        var request = new { Email = "admin@sysvet.com", Password = "WrongPassword123!" };

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithValidTokens_ReturnsNewTokens()
    {
        await SeedUserAsync();
        var client = _factory.CreateClient();

        var loginRequest = new { Email = "admin@sysvet.com", Password = "Password123!" };
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        var loginContent = await loginResponse.Content.ReadFromJsonAsync<AuthTokensResponse>();

        var refreshRequest = new { RefreshToken = loginContent!.RefreshToken };

        var refreshResponse = await client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshContent = await refreshResponse.Content.ReadFromJsonAsync<AuthTokensResponse>();
        refreshContent.Should().NotBeNull();
        refreshContent!.AccessToken.Should().NotBeNullOrEmpty();
        refreshContent.RefreshToken.Should().NotBeNullOrEmpty();

        refreshContent.AccessToken.Should().NotBe(loginContent.AccessToken);
        refreshContent.RefreshToken.Should().NotBe(loginContent.RefreshToken);
    }

    [Fact]
    public async Task Refresh_WithReusedToken_ReturnsUnauthorized()
    {
        await SeedUserAsync();
        var client = _factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = "admin@sysvet.com", Password = "Password123!" });
        var loginContent = await loginResponse.Content.ReadFromJsonAsync<AuthTokensResponse>();
        var oldRefresh = loginContent!.RefreshToken;

        var firstRefresh = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { RefreshToken = oldRefresh });
        firstRefresh.StatusCode.Should().Be(HttpStatusCode.OK);

        var reuse = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { RefreshToken = oldRefresh });
        reuse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/auth/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_WithValidToken_ReturnsProfile()
    {
        await SeedUserAsync();
        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = "admin@sysvet.com", Password = "Password123!" });
        var tokens = await loginResponse.Content.ReadFromJsonAsync<AuthTokensResponse>();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens!.AccessToken);

        var response = await client.GetAsync("/api/v1/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var me = await response.Content.ReadFromJsonAsync<MeResponse>();
        me.Should().NotBeNull();
        me!.Email.Should().Be("admin@sysvet.com");
        me.Roles.Should().Contain("Admin");
        me.TenantId.Should().NotBe(Guid.Empty.ToString());
    }

    [Fact]
    public async Task Register_InDevelopment_CreatesUserAndAllowsLogin()
    {
        await SeedUserAsync();
        var client = _factory.CreateClient();
        var register = new { Email = "newdev@sysvet.com", Password = "Password123!", Role = "Receptionist" };

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", register);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = "newdev@sysvet.com", Password = "Password123!" });
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

public class AuthTokensResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresInSeconds { get; set; }
}

public class MeResponse
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public IEnumerable<string> Roles { get; set; } = [];
}

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
