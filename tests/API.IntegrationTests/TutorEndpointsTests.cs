using System.Net;
using System.Net.Http.Json;
using Core.Application.Tutors.Commands;
using Core.Domain.Entities;
using Core.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace API.IntegrationTests;

[Collection("IntegrationTests")]
public class TutorEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public TutorEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        using var scope = _factory.Services.CreateScope();
        
        var context = scope.ServiceProvider.GetRequiredService<Core.Infrastructure.Persistence.CoreDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        var user = await userManager.FindByEmailAsync("test@sysvet.com");
        if (user == null)
        {
            user = new AppUser { UserName = "test@sysvet.com", Email = "test@sysvet.com", TenantId = Guid.NewGuid() };
            await userManager.CreateAsync(user, "Password123!");
            await userManager.AddToRoleAsync(user, "Admin");
        }

        var client = _factory.CreateClient();
        var request = new { Email = "test@sysvet.com", Password = "Password123!" };
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", request);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse!.AccessToken);
        return client;
    }

    [Fact]
    public async Task RegisterTutor_WithValidData_ReturnsCreated()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();
        var command = new RegisterTutorCommand(Guid.NewGuid(), "John Doe", "john@doe.com", "63683891416", "11999999999");

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/tutors", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task RegisterTutor_WithSameIdempotencyKey_ShouldBeIdempotent()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();
        var command = new RegisterTutorCommand(Guid.NewGuid(), "Jane Doe", "jane@doe.com", "10125103360", "11988888888");
        var idempotencyKey = Guid.NewGuid().ToString();
        
        var requestMessage1 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/tutors")
        {
            Content = JsonContent.Create(command)
        };
        requestMessage1.Headers.Add("Idempotency-Key", idempotencyKey);

        var requestMessage2 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/tutors")
        {
            Content = JsonContent.Create(command)
        };
        requestMessage2.Headers.Add("Idempotency-Key", idempotencyKey);

        // Act
        var response1 = await client.SendAsync(requestMessage1);
        var response2 = await client.SendAsync(requestMessage2);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.Created); // Because it was intercepted by IdempotencyBehavior and returned Success Result!
    }

    [Fact]
    public async Task ListTutors_ReturnsOk()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/v1/tutors");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
