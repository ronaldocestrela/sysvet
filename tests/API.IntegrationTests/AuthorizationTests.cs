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
public class AuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthorizationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private async Task SeedUserAsync(string roleName)
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
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string roleName)
    {
        await SeedUserAsync(roleName);
        var client = _factory.CreateClient();
        
        var loginRequest = new { Email = $"{roleName}@sysvet.com", Password = "Password123!" };
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        var loginContent = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginContent!.AccessToken);
        return client;
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Veterinarian")]
    [InlineData("Receptionist")]
    public async Task GetTutors_WithAllowedRoles_ReturnsOk(string role)
    {
        var client = await CreateAuthenticatedClientAsync(role);
        var response = await client.GetAsync("/api/v1/tutors?pageNumber=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTutors_WithForbiddenRole_ReturnsForbidden()
    {
        var client = await CreateAuthenticatedClientAsync("Cashier");
        var response = await client.GetAsync("/api/v1/tutors?pageNumber=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
