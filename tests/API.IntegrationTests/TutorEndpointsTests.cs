using System.Net;
using System.Net.Http.Json;
using Core.Application.Common;
using Core.Application.Tutors.Commands;
using Core.Application.Tutors.Queries;
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
    public async Task CreateTutor_WithValidData_ReturnsCreated()
    {
        var client = await CreateAuthenticatedClientAsync();
        var command = new CreateTutorCommand(Guid.NewGuid(), "John Doe", "john@doe.com", "63683891416", "11999999999");

        var response = await client.PostAsJsonAsync("/api/v1/tutors", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateTutor_WithDuplicateCpf_ReturnsConflict()
    {
        var client = await CreateAuthenticatedClientAsync();
        var cpf = "63683891416";
        await client.PostAsJsonAsync("/api/v1/tutors", new CreateTutorCommand(Guid.NewGuid(), "John Doe", "john@doe.com", cpf, "11999999999"));
        var response = await client.PostAsJsonAsync("/api/v1/tutors", new CreateTutorCommand(Guid.NewGuid(), "Jane Doe", "jane@doe.com", cpf, "11888888888"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateTutor_WithSameIdempotencyKey_ShouldBeIdempotent()
    {
        var client = await CreateAuthenticatedClientAsync();
        var command = new CreateTutorCommand(Guid.NewGuid(), "Jane Doe", "jane@doe.com", "10125103360", "11988888888");
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

        var response1 = await client.SendAsync(requestMessage1);
        var response2 = await client.SendAsync(requestMessage2);

        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task ListTutors_WithNameFilter_ReturnsPagedResults()
    {
        var client = await CreateAuthenticatedClientAsync();
        await client.PostAsJsonAsync("/api/v1/tutors", new CreateTutorCommand(Guid.NewGuid(), "Alice Search", "alice@doe.com", "52998224725", "11999999999"));

        var response = await client.GetAsync("/api/v1/tutors?nameFilter=Alice");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<TutorDto>>();
        page!.Items.Should().ContainSingle(t => t.Name.Contains("Alice"));
    }

    [Fact]
    public async Task DeleteTutor_ThenGet_ReturnsNotFound()
    {
        var client = await CreateAuthenticatedClientAsync();
        var id = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/v1/tutors", new CreateTutorCommand(id, "To Delete", "del@doe.com", "11144477735", "11999999999"));

        var deleteResponse = await client.DeleteAsync($"/api/v1/tutors/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/v1/tutors/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
