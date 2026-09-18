using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Core.Domain.Entities;
using Core.Infrastructure.Identity;

namespace API.IntegrationTests.Veterinary;

[Collection("IntegrationTests")]
public class HospitalizationEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HospitalizationEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        using var scope = _factory.Services.CreateScope();

        var coreContext = scope.ServiceProvider.GetRequiredService<Core.Infrastructure.Persistence.CoreDbContext>();
        await coreContext.Database.EnsureDeletedAsync();
        await coreContext.Database.EnsureCreatedAsync();

        var vetContext = scope.ServiceProvider.GetRequiredService<global::Veterinary.Infrastructure.Persistence.VeterinaryDbContext>();
        await vetContext.Database.MigrateAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        var user = await userManager.FindByEmailAsync("test@sysvet.com");
        if (user == null)
        {
            user = new AppUser { UserName = "test@sysvet.com", Email = "test@sysvet.com", TenantId = Guid.NewGuid() };
            await userManager.CreateAsync(user, "Password123!");
            await userManager.AddToRoleAsync(user, "Admin");
        }

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = "test@sysvet.com", Password = "Password123!" });
        var loginResult = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var token = loginResult.GetProperty("accessToken").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task ExecutionMapLifecycle_ShouldSucceed()
    {
        var client = await CreateAuthenticatedClientAsync();
        var petId = await SeedPetAsync(client);
        var veterinarianId = Guid.NewGuid();
        var bedId = Guid.NewGuid();

        var wardResponse = await client.PostAsJsonAsync("/api/v1/ward-units", new
        {
            Name = "ICU",
            Beds = new[] { new { BedId = (Guid?)bedId, Code = "A1", SortOrder = 0, IsActive = true } }
        });
        wardResponse.EnsureSuccessStatusCode();

        var admitResponse = await client.PostAsJsonAsync("/api/v1/hospitalizations", new
        {
            PetId = petId,
            VeterinarianId = veterinarianId,
            BedId = bedId,
            Reason = "Severe vomiting"
        });
        admitResponse.EnsureSuccessStatusCode();
        var hospitalizationId = await admitResponse.Content.ReadFromJsonAsync<Guid>();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var orderResponse = await client.PostAsJsonAsync($"/api/v1/hospitalizations/{hospitalizationId}/medication-orders", new
        {
            MedicationName = "Ondansetron",
            Dose = "4mg",
            Route = "IV",
            DailyTimeStrings = new[] { "08:00:00" },
            StartsOn = today,
            EndsOn = today
        });
        orderResponse.EnsureSuccessStatusCode();

        var mapResponse = await client.GetAsync($"/api/v1/hospitalizations/execution-map?date={today:yyyy-MM-dd}");
        mapResponse.EnsureSuccessStatusCode();
        var mapJson = await mapResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        mapJson.GetProperty("wards").GetArrayLength().Should().BeGreaterThan(0);

        var adminId = mapJson.GetProperty("wards")[0].GetProperty("beds")[0]
            .GetProperty("occupancy").GetProperty("administrations")[0].GetProperty("id").GetGuid();

        var administerResponse = await client.PostAsJsonAsync(
            $"/api/v1/hospitalizations/{hospitalizationId}/administrations/{adminId}/administer",
            new { Notes = "OK" });
        administerResponse.EnsureSuccessStatusCode();

        var dischargeResponse = await client.PostAsync($"/api/v1/hospitalizations/{hospitalizationId}/discharge", null);
        dischargeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private static async Task<Guid> SeedPetAsync(HttpClient client)
    {
        var tutorResponse = await client.PostAsJsonAsync("/api/v1/tutors", new
        {
            Name = "Tutor Hosp",
            Email = $"tutor-hosp-{Guid.NewGuid():N}@test.com",
            Cpf = "52998224725",
            Phone = "11999999999"
        });
        tutorResponse.EnsureSuccessStatusCode();
        var tutorId = Guid.Parse((await tutorResponse.Content.ReadAsStringAsync()).Trim('"'));

        var petResponse = await client.PostAsJsonAsync("/api/v1/pets", new
        {
            Name = "Rex",
            Species = PetSpecies.Dog,
            Breed = "SRD",
            Sex = PetSex.Male,
            TutorId = tutorId
        });
        petResponse.EnsureSuccessStatusCode();
        return Guid.Parse((await petResponse.Content.ReadAsStringAsync()).Trim('"'));
    }
}
