using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Core.Application.Authorization;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using Core.Infrastructure.Identity;
using Core.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TutorPortal.Application.PetHealth.Dtos;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Enums;
using Veterinary.Infrastructure.Persistence;
using Xunit;

namespace API.IntegrationTests.TutorPortal;

[Collection("IntegrationTests")]
public class TutorPortalPetHealthEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string TutorEmail = "tutor.health@test.com";
    private const string TutorCpf = "52998224725";
    private const string TutorPassword = "Password123!";

    private readonly WebApplicationFactory<Program> _factory;

    public TutorPortalPetHealthEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Tutor_ViewsVaccinesAndExams_OfAuthorizedPet()
    {
        var petId = await SeedPetWithHealthDataAsync();
        var client = await CreateTutorClientAsync();

        var cardResponse = await client.GetAsync($"/api/v1/tutor-portal/pets/{petId}/vaccination-card");
        cardResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var card = await cardResponse.Content.ReadFromJsonAsync<TutorVaccinationCardDto>();
        card!.PetId.Should().Be(petId);
        card.AppliedDoses.Should().ContainSingle(d => d.Name == "Raiva");

        var examsResponse = await client.GetAsync($"/api/v1/tutor-portal/pets/{petId}/exams");
        examsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var exams = await examsResponse.Content.ReadFromJsonAsync<List<TutorPetExamDto>>();
        exams!.Should().ContainSingle(e => e.Name == "Hemograma" && e.Status == "Completed");
    }

    [Fact]
    public async Task Tutor_ForbiddenOnUnauthorizedPet()
    {
        await SeedPetWithHealthDataAsync();
        var otherPetId = await SeedOtherTutorPetAsync();
        var client = await CreateTutorClientAsync();

        var response = await client.GetAsync($"/api/v1/tutor-portal/pets/{otherPetId}/vaccination-card");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ClinicStaffToken_ForbiddenOnTutorPetCard()
    {
        var petId = await SeedPetWithHealthDataAsync();
        var staffClient = await CreateStaffClientAsync();

        var response = await staffClient.GetAsync($"/api/v1/tutor-portal/pets/{petId}/vaccination-card");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task<Guid> SeedPetWithHealthDataAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        await PrepareDatabasesAsync(scope);

        var core = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
        var tutor = await core.Tutors.FirstOrDefaultAsync(t => t.Email.Address == TutorEmail);
        if (tutor is null)
        {
            tutor = Tutor.Create(
                "Portal Tutor",
                Email.Create(TutorEmail).Value,
                Cpf.Create(TutorCpf).Value,
                Phone.Create("11999997777").Value).Value;
            core.Tutors.Add(tutor);
            await core.SaveChangesAsync();
        }

        var pet = Pet.Create("Rex", PetSpecies.Dog, "SRD", PetSex.Male, tutor.Id).Value;
        core.Pets.Add(pet);
        await core.SaveChangesAsync();

        var vetContext = scope.ServiceProvider.GetRequiredService<VeterinaryDbContext>();
        var dose = VaccineDose.Create(
            Guid.NewGuid(),
            pet.Id,
            "Raiva",
            "LOT1",
            DateTimeOffset.UtcNow.AddDays(-30),
            DateTimeOffset.UtcNow.AddDays(335)).Value;
        vetContext.VaccineDoses.Add(dose);

        var appointmentId = Guid.NewGuid();
        var appointment = Appointment.RestoreFromSync(
            appointmentId,
            tutor.Id,
            pet.Id,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(-2),
            30,
            "Consulta",
            AppointmentStatus.Completed,
            DateTimeOffset.UtcNow);
        vetContext.Appointments.Add(appointment);

        var exam = ClinicalExam.Request(Guid.NewGuid(), appointmentId, pet.Id, "Hemograma", ClinicalExamCategory.Laboratory).Value;
        exam.Complete("Within range");
        vetContext.ClinicalExams.Add(exam);

        await vetContext.SaveChangesAsync();
        return pet.Id;
    }

    private async Task<Guid> SeedOtherTutorPetAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var core = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
        var other = Tutor.Create(
            "Other Tutor",
            Email.Create($"other-{Guid.NewGuid():N}@test.com").Value,
            Cpf.Create("39053344705").Value,
            Phone.Create("11988887777").Value).Value;
        core.Tutors.Add(other);
        var pet = Pet.Create("Other", PetSpecies.Cat, "SRD", PetSex.Female, other.Id).Value;
        core.Pets.Add(pet);
        await core.SaveChangesAsync();
        return pet.Id;
    }

    private async Task<HttpClient> CreateTutorClientAsync()
    {
        var client = _factory.CreateClient();
        var registerResponse = await client.PostAsJsonAsync(
            "/api/v1/tutor-portal/register",
            new { Email = TutorEmail, Cpf = TutorCpf, Password = TutorPassword });
        if (registerResponse.StatusCode != HttpStatusCode.OK)
        {
            var loginResponse = await client.PostAsJsonAsync(
                "/api/v1/tutor-portal/login",
                new { Email = TutorEmail, Password = TutorPassword });
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var loginTokens = await loginResponse.Content.ReadFromJsonAsync<AuthTokensResponse>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginTokens!.AccessToken);
            return client;
        }

        var tokens = await registerResponse.Content.ReadFromJsonAsync<AuthTokensResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);
        return client;
    }

    private async Task<HttpClient> CreateStaffClientAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        await PrepareDatabasesAsync(scope);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in ApplicationRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var email = $"staff-{Guid.NewGuid():N}@sysvet.com";
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            TenantId = IntegrationTestDatabaseHelper.SingleTenantId
        };
        await userManager.CreateAsync(user, TutorPassword);
        await userManager.AddToRoleAsync(user, ApplicationRoles.Admin);

        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = TutorPassword });
        var tokens = await login.Content.ReadFromJsonAsync<AuthTokensResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);
        return client;
    }

    private static async Task PrepareDatabasesAsync(IServiceScope scope)
    {
        var coreContext = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
        await coreContext.Database.EnsureDeletedAsync();
        await coreContext.Database.MigrateAsync();
        await IntegrationTestDatabaseHelper.MigrateModuleDatabasesAsync(scope);

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in ApplicationRoles.AllIncludingTutor)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private sealed record AuthTokensResponse(string AccessToken, string RefreshToken, int ExpiresInSeconds);
}
