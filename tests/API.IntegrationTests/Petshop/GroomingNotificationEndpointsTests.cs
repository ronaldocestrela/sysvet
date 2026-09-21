using System.Net.Http.Headers;
using System.Net.Http.Json;
using Core.Application.Notifications;
using Core.Application.Pets.Commands;
using Core.Application.Tutors.Commands;
using Core.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Petshop.Application.GroomingServices;
using Petshop.Domain.Enums;
using Xunit;

namespace API.IntegrationTests.Petshop;

[Collection("IntegrationTests")]
public class GroomingNotificationEndpointsTests
{
    [Fact]
    public async Task MarkReady_WhenAutomationsChannelEnabled_NotifiesTutor()
    {
        var channel = new CapturingTutorNotificationChannel();
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ITutorNotificationChannel>();
                services.AddSingleton(channel);
                services.AddSingleton<ITutorNotificationChannel>(sp => sp.GetRequiredService<CapturingTutorNotificationChannel>());
            });
        });

        var client = await CreateAuthenticatedClientAsync(factory);
        var tutorId = Guid.NewGuid();
        var petId = Guid.NewGuid();
        var groomerId = Guid.NewGuid();

        (await client.PostAsJsonAsync("/api/v1/tutors", new CreateTutorCommand(
            tutorId, "Tutor Banho", "banho@sysvet.com", "52998224725", "11988887777"))).EnsureSuccessStatusCode();
        (await client.PostAsJsonAsync("/api/v1/pets", new CreatePetCommand("Thor", PetSpecies.Dog, "SRD", PetSex.Male, tutorId, Id: petId)))
            .EnsureSuccessStatusCode();

        var serviceId = Guid.NewGuid();
        (await client.PutAsJsonAsync("/api/v1/grooming-services", new UpsertGroomingServiceCommand(
            serviceId, "Banho", GroomingServiceType.Banho, 45, null, true, [], Guid.Empty))).EnsureSuccessStatusCode();

        var day = DateTimeOffset.UtcNow.AddDays(1).Date;
        var slotDate = new DateTimeOffset(day, TimeSpan.Zero);
        (await client.PostAsJsonAsync("/api/v1/grooming-slots/availability", new
        {
            groomerId,
            date = slotDate,
            dayStart = "08:00:00",
            dayEnd = "18:00:00",
            slotDurationMinutes = 60
        })).EnsureSuccessStatusCode();

        var appointmentId = Guid.NewGuid();
        (await client.PostAsJsonAsync("/api/v1/grooming-appointments", new
        {
            id = appointmentId,
            tutorId,
            petId,
            groomerId,
            groomingServiceId = serviceId,
            date = slotDate.AddHours(10),
            durationInMinutes = 45,
            notes = ""
        })).EnsureSuccessStatusCode();

        (await client.PostAsync($"/api/v1/grooming-appointments/{appointmentId}/confirm", null)).EnsureSuccessStatusCode();
        (await client.PostAsync($"/api/v1/grooming-appointments/{appointmentId}/start", null)).EnsureSuccessStatusCode();
        (await client.PostAsync($"/api/v1/grooming-appointments/{appointmentId}/ready", null)).EnsureSuccessStatusCode();

        channel.Notifications.Should().ContainSingle(n =>
            n.TutorId == tutorId &&
            n.GroomingAppointmentId == appointmentId &&
            n.Kind == Core.Application.IntegrationEvents.GroomingNotificationKind.ReadyForPickup);
    }

    private static async Task<HttpClient> CreateAuthenticatedClientAsync(WebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var coreContext = scope.ServiceProvider.GetRequiredService<Core.Infrastructure.Persistence.CoreDbContext>();
        await coreContext.Database.EnsureDeletedAsync();
        await coreContext.Database.MigrateAsync();
        await IntegrationTestDatabaseHelper.MigrateModuleDatabasesAsync(scope);

        var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Core.Infrastructure.Identity.AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole("Admin"));
        }

        var email = $"grooming-notify-{Guid.NewGuid():N}@sysvet.com";
        var user = new Core.Infrastructure.Identity.AppUser { UserName = email, Email = email, TenantId = Guid.NewGuid() };
        await userManager.CreateAsync(user, "Password123!");
        await userManager.AddToRoleAsync(user, "Admin");

        var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = "Password123!" });
        var token = (await login.Content.ReadFromJsonAsync<LoginResponseDto>())!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private sealed class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
    }
}
