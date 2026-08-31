using System.Net;
using System.Net.Http.Json;
using Core.Domain.Auditing;
using Core.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
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

    [Fact]
    public async Task AuditLog_IsCreated_WhenEntityIsAdded()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("Admin");
        var command = new Core.Application.Tutors.Commands.RegisterTutorCommand(Guid.NewGuid(), "John Doe", "john@example.com", "63683891416", "11999999999");

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/tutors", command);

        var content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, content);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Core.Infrastructure.Persistence.CoreDbContext>();
        
        var logs = await dbContext.AuditLogs.ToListAsync();
        logs.Should().Contain(l => l.EntityName == "Tutor" && l.Action == "Added");
    }
}
