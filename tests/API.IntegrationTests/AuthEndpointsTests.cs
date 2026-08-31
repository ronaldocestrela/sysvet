using System.Net;
using System.Net.Http.Json;
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

        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        var user = await userManager.FindByEmailAsync("admin@sysvet.com");
        if (user == null)
        {
            user = new AppUser { UserName = "admin@sysvet.com", Email = "admin@sysvet.com", TenantId = Guid.NewGuid() };
            await userManager.CreateAsync(user, "Password123!");
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsJwtToken()
    {
        // Arrange
        await SeedUserAsync();
        var client = _factory.CreateClient();
        var request = new { Email = "admin@sysvet.com", Password = "Password123!" };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<LoginResponse>();
        content.Should().NotBeNull();
        content!.AccessToken.Should().NotBeNullOrEmpty();
        content.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        await SeedUserAsync();
        var client = _factory.CreateClient();
        var request = new { Email = "admin@sysvet.com", Password = "WrongPassword123!" };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithValidTokens_ReturnsNewTokens()
    {
        // Arrange
        await SeedUserAsync();
        var client = _factory.CreateClient();
        
        // 1. Login first
        var loginRequest = new { Email = "admin@sysvet.com", Password = "Password123!" };
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        var loginContent = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        
        // 2. Refresh
        var refreshRequest = new { AccessToken = loginContent!.AccessToken, RefreshToken = loginContent.RefreshToken };
        
        // Act
        var refreshResponse = await client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);
        
        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshContent = await refreshResponse.Content.ReadFromJsonAsync<LoginResponse>();
        refreshContent.Should().NotBeNull();
        refreshContent!.AccessToken.Should().NotBeNullOrEmpty();
        refreshContent.RefreshToken.Should().NotBeNullOrEmpty();
        
        refreshContent.AccessToken.Should().NotBe(loginContent.AccessToken);
        refreshContent.RefreshToken.Should().NotBe(loginContent.RefreshToken);
    }


    [Fact]
    public async Task GetMe_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
