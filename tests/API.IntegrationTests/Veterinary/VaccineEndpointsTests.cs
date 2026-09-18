using System.Net.Http.Headers;
using System.Net.Http.Json;
using Core.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Veterinary.Application.Vaccines.Dtos;
using Xunit;

namespace API.IntegrationTests.Veterinary;

[Collection("IntegrationTests")]
public class VaccineEndpointsTests : IClassFixture<VaccineWebApplicationFactory>
{
    private readonly VaccineWebApplicationFactory _factory;

    public VaccineEndpointsTests(VaccineWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task RegisterVaccine_ListCardAndAlerts_Succeeds()
    {
        var client = await CreateAuthenticatedClientAsync();
        var petId = await SeedPetAsync(client);

        var protocolResponse = await client.PostAsJsonAsync("/api/v1/vaccine-protocols", new
        {
            Name = "Canine Core",
            Species = PetSpecies.Dog,
            Doses = new[]
            {
                new { DoseId = (Guid?)null, Label = "1ª dose", MinAgeInDays = 0, MaxAgeInDays = (int?)null, IntervalFromPreviousInDays = (int?)null, NextDoseIntervalInDays = (int?)365 }
            }
        });
        protocolResponse.IsSuccessStatusCode.Should().BeTrue();

        var registerResponse = await client.PostAsJsonAsync($"/api/v1/pets/{petId}/vaccines", new
        {
            Name = "Raiva",
            BatchNumber = "LOT1",
            AppliedAt = DateTimeOffset.UtcNow.AddDays(-10),
            NextDueDate = DateTimeOffset.UtcNow.AddDays(-1),
            ProtocolDoseId = (Guid?)null,
            Id = (Guid?)null
        });
        registerResponse.IsSuccessStatusCode.Should().BeTrue();

        var listResponse = await client.GetAsync($"/api/v1/pets/{petId}/vaccines");
        listResponse.IsSuccessStatusCode.Should().BeTrue(await listResponse.Content.ReadAsStringAsync());

        var cardResponse = await client.GetAsync($"/api/v1/pets/{petId}/vaccination-card");
        cardResponse.IsSuccessStatusCode.Should().BeTrue(await cardResponse.Content.ReadAsStringAsync());
        var card = await cardResponse.Content.ReadFromJsonAsync<VaccinationCardDto>();
        card!.PetId.Should().Be(petId);
        card.AppliedDoses.Should().NotBeEmpty();

        var alertsResponse = await client.GetAsync("/api/v1/vaccine-alerts?status=Overdue");
        alertsResponse.IsSuccessStatusCode.Should().BeTrue(await alertsResponse.Content.ReadAsStringAsync());
        var alerts = await alertsResponse.Content.ReadFromJsonAsync<List<VaccineAlertDto>>();
        alerts!.Should().Contain(a => a.PetId == petId);
    }

    private async Task<Guid> SeedPetAsync(HttpClient client)
    {
        var tutorResponse = await client.PostAsJsonAsync("/api/v1/tutors", new
        {
            Name = "Tutor Vac",
            Email = $"tutor-vac-{Guid.NewGuid():N}@test.com",
            Cpf = "52998224725",
            Phone = "11999999999"
        });
        tutorResponse.IsSuccessStatusCode.Should().BeTrue();
        var tutorId = Guid.Parse((await tutorResponse.Content.ReadAsStringAsync()).Trim('"'));

        var petResponse = await client.PostAsJsonAsync("/api/v1/pets", new
        {
            Name = "Rex",
            Species = PetSpecies.Dog,
            Breed = "SRD",
            Sex = PetSex.Male,
            TutorId = tutorId,
            BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        });
        petResponse.IsSuccessStatusCode.Should().BeTrue();
        return Guid.Parse((await petResponse.Content.ReadAsStringAsync()).Trim('"'));
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var coreContext = scope.ServiceProvider.GetRequiredService<Core.Infrastructure.Persistence.CoreDbContext>();
        await coreContext.Database.EnsureDeletedAsync();
        await coreContext.Database.EnsureCreatedAsync();

        var vetContext = scope.ServiceProvider.GetRequiredService<global::Veterinary.Infrastructure.Persistence.VeterinaryDbContext>();
        await vetContext.Database.MigrateAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Core.Infrastructure.Identity.AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole("Admin"));
        }

        var user = await userManager.FindByEmailAsync("vet-vaccine@sysvet.com");
        if (user is null)
        {
            user = new Core.Infrastructure.Identity.AppUser { UserName = "vet-vaccine@sysvet.com", Email = "vet-vaccine@sysvet.com", TenantId = Guid.NewGuid() };
            await userManager.CreateAsync(user, "Password123!");
            await userManager.AddToRoleAsync(user, "Admin");
        }

        var httpClient = _factory.CreateClient();
        var loginResponse = await httpClient.PostAsJsonAsync("/api/v1/auth/login", new { Email = "vet-vaccine@sysvet.com", Password = "Password123!" });
        loginResponse.IsSuccessStatusCode.Should().BeTrue();
        var tokens = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);
        return httpClient;
    }

    private sealed record LoginResponse(string AccessToken, string RefreshToken);
}
