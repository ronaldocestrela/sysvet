using System.Net;
using System.Net.Http.Json;
using Core.Application.Common;
using Core.Application.Pets.Commands;
using Core.Application.Pets.Queries;
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
public class PetEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PetEndpointsTests(WebApplicationFactory<Program> factory)
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

        var user = await userManager.FindByEmailAsync("petadmin@sysvet.com");
        if (user == null)
        {
            user = new AppUser { UserName = "petadmin@sysvet.com", Email = "petadmin@sysvet.com", TenantId = Guid.NewGuid() };
            await userManager.CreateAsync(user, "Password123!");
            await userManager.AddToRoleAsync(user, "Admin");
        }

        var client = _factory.CreateClient();
        var request = new { Email = "petadmin@sysvet.com", Password = "Password123!" };
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", request);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse!.AccessToken);
        return client;
    }

    [Fact]
    public async Task CreatePet_WithValidData_ReturnsCreated()
    {
        var client = await CreateAuthenticatedClientAsync();

        var tutorCommand = new CreateTutorCommand(Guid.NewGuid(), "Pet Owner", "owner2@pets.com", "11122233396", "11999999999");
        var tutorResponse = await client.PostAsJsonAsync("/api/v1/tutors", tutorCommand);
        tutorResponse.EnsureSuccessStatusCode();

        var tutorId = tutorCommand.Id;
        var command = new CreatePetCommand("Rex", PetSpecies.Dog, "Poodle", PetSex.Male, tutorId);

        var response = await client.PostAsJsonAsync("/api/v1/pets", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task ListPets_WithNameFilter_ReturnsMatches()
    {
        var client = await CreateAuthenticatedClientAsync();
        var tutorId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/v1/tutors", new CreateTutorCommand(tutorId, "Owner", "owner@pets.com", "11122233396", "11999999999"));
        await client.PostAsJsonAsync("/api/v1/pets", new CreatePetCommand("Rex Search", PetSpecies.Dog, "Poodle", PetSex.Male, tutorId));

        var response = await client.GetAsync("/api/v1/pets?nameFilter=Rex");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<PetDto>>();
        page!.Items.Should().ContainSingle(p => p.Name.Contains("Rex"));
    }
}
