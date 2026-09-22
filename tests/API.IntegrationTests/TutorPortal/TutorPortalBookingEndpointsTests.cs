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
using TutorPortal.Application.Scheduling;
using TutorPortal.Application.Scheduling.Dtos;
using Veterinary.Application.Appointments.DTOs;
using Veterinary.Domain.Entities;
using Veterinary.Infrastructure.Persistence;

namespace API.IntegrationTests.TutorPortal;

[Collection("IntegrationTests")]
public class TutorPortalBookingEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string TutorEmail = "tutor.booking@test.com";
    private const string TutorCpf = "52998224725";
    private const string TutorPassword = "Password123!";

    private readonly WebApplicationFactory<Program> _factory;

    public TutorPortalBookingEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Tutor_BooksAppointment_AppearsOnClinicDailySchedule()
    {
        var (petId, vetId, day, slotStart) = await SeedTutorPetAndVetSlotsAsync();
        var tutorClient = await CreateTutorClientAsync();
        var appointmentId = Guid.NewGuid();

        var bookResponse = await tutorClient.PostAsJsonAsync(
            $"/api/v1/tutor-portal/pets/{petId}/booking/appointments",
            new
            {
                kind = TutorBookingKind.Clinical,
                serviceId = TutorBookingConstants.ClinicalConsultationServiceId,
                professionalId = vetId,
                date = slotStart,
                id = appointmentId
            });
        bookResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var bookedId = await bookResponse.Content.ReadFromJsonAsync<Guid>();
        bookedId.Should().Be(appointmentId);

        var staffClient = await CreateStaffClientAsync(prepareDatabase: false);
        var daily = await staffClient.GetFromJsonAsync<List<AppointmentDto>>(
            $"/api/v1/appointments/daily?veterinarianId={vetId}&date={Uri.EscapeDataString(day.ToString("O"))}");
        daily!.Should().ContainSingle(a => a.Id == appointmentId);
    }

    [Fact]
    public async Task Tutor_CancelBooking_ReleasesSlotForRebook()
    {
        var (petId, vetId, day, slotStart) = await SeedTutorPetAndVetSlotsAsync();
        var tutorClient = await CreateTutorClientAsync();
        var appointmentId = Guid.NewGuid();

        var bookResponse = await tutorClient.PostAsJsonAsync(
            $"/api/v1/tutor-portal/pets/{petId}/booking/appointments",
            new
            {
                kind = TutorBookingKind.Clinical,
                serviceId = TutorBookingConstants.ClinicalConsultationServiceId,
                professionalId = vetId,
                date = slotStart,
                id = appointmentId
            });
        bookResponse.EnsureSuccessStatusCode();

        var cancelResponse = await tutorClient.PostAsJsonAsync(
            $"/api/v1/tutor-portal/pets/{petId}/booking/appointments/{appointmentId}/cancel",
            new { kind = TutorBookingKind.Clinical });
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var rebookId = Guid.NewGuid();
        var rebookResponse = await tutorClient.PostAsJsonAsync(
            $"/api/v1/tutor-portal/pets/{petId}/booking/appointments",
            new
            {
                kind = TutorBookingKind.Clinical,
                serviceId = TutorBookingConstants.ClinicalConsultationServiceId,
                professionalId = vetId,
                date = slotStart,
                id = rebookId
            });
        rebookResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ClinicStaffToken_ForbiddenOnTutorBookingServices()
    {
        var (petId, _, _, _) = await SeedTutorPetAndVetSlotsAsync();
        var staffClient = await CreateStaffClientAsync();

        var response = await staffClient.GetAsync($"/api/v1/tutor-portal/pets/{petId}/booking/services");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Tutor_ForbiddenOnStaffScheduleAppointment()
    {
        await SeedTutorPetAndVetSlotsAsync();
        var tutorClient = await CreateTutorClientAsync();

        var response = await tutorClient.PostAsJsonAsync("/api/v1/appointments", new
        {
            id = Guid.NewGuid(),
            tutorId = Guid.NewGuid(),
            petId = Guid.NewGuid(),
            veterinarianId = Guid.NewGuid(),
            date = DateTimeOffset.UtcNow.AddDays(2),
            durationInMinutes = 30,
            reason = "Test"
        });
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Tutor_BookingServices_ReturnsClinicalOffering()
    {
        var (petId, _, _, _) = await SeedTutorPetAndVetSlotsAsync();
        var tutorClient = await CreateTutorClientAsync();

        var response = await tutorClient.GetAsync($"/api/v1/tutor-portal/pets/{petId}/booking/services");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var services = await response.Content.ReadFromJsonAsync<List<TutorBookableServiceDto>>();
        services!.Should().Contain(s => s.ServiceId == TutorBookingConstants.ClinicalConsultationServiceId);
    }

    private async Task<(Guid PetId, Guid VetId, DateTimeOffset Day, DateTimeOffset SlotStart)> SeedTutorPetAndVetSlotsAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        await PrepareDatabasesAsync(scope);
        SetTenantContext(scope);

        var core = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
        var tutor = await core.Tutors.FirstOrDefaultAsync(t => t.Email.Address == TutorEmail);
        if (tutor is null)
        {
            tutor = Tutor.Create(
                "Booking Tutor",
                Email.Create(TutorEmail).Value,
                Cpf.Create(TutorCpf).Value,
                Phone.Create("11999997777").Value).Value;
            core.Tutors.Add(tutor);
            await core.SaveChangesAsync();
        }

        var pet = Pet.Create("Rex", PetSpecies.Dog, "SRD", PetSex.Male, tutor.Id).Value;
        core.Pets.Add(pet);
        await core.SaveChangesAsync();

        var vetId = Guid.NewGuid();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var vetUser = new AppUser
        {
            Id = vetId.ToString(),
            UserName = $"vet-{vetId:N}@sysvet.com",
            Email = $"vet-{vetId:N}@sysvet.com",
            TenantId = IntegrationTestDatabaseHelper.SingleTenantId,
            DisplayName = "Dr. Vet"
        };
        await userManager.CreateAsync(vetUser, TutorPassword);
        await userManager.AddToRoleAsync(vetUser, ApplicationRoles.Veterinarian);

        var day = DateTimeOffset.UtcNow.AddDays(2).Date;
        var slotDate = new DateTimeOffset(day, TimeSpan.Zero);
        var vetContext = scope.ServiceProvider.GetRequiredService<VeterinaryDbContext>();
        vetContext.ScheduleSlots.Add(new ScheduleSlot(
            Guid.NewGuid(),
            vetId,
            slotDate,
            TimeSpan.FromHours(9),
            TimeSpan.FromHours(9).Add(TimeSpan.FromMinutes(30))));
        await vetContext.SaveChangesAsync();

        var slotStart = slotDate.AddHours(9);
        return (pet.Id, vetId, slotDate, slotStart);
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

    private async Task<HttpClient> CreateStaffClientAsync(bool prepareDatabase = true)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        if (prepareDatabase)
        {
            await PrepareDatabasesAsync(scope);
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in ApplicationRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var email = $"staff-booking-{Guid.NewGuid():N}@sysvet.com";
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

    private static void SetTenantContext(IServiceScope scope)
    {
        var tenantContext = scope.ServiceProvider.GetRequiredService<Core.Domain.ITenantContext>();
        tenantContext.TenantId = IntegrationTestDatabaseHelper.SingleTenantId;
        tenantContext.SchemaName = $"tenant_{IntegrationTestDatabaseHelper.SingleTenantId:N}".ToLowerInvariant();
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
