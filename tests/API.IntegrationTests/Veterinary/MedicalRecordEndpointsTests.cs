using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Veterinary.Application.MedicalRecords.DTOs;
using MedicalRecordDto = Veterinary.Application.MedicalRecords.DTOs.MedicalRecordDto;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Enums;
using Xunit;

namespace API.IntegrationTests.Veterinary;

[Collection("IntegrationTests")]
public class MedicalRecordEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public MedicalRecordEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task CreateAndTimeline_ForEligibleAppointment_Succeeds()
    {
        var client = await CreateAuthenticatedClientAsync();
        using var scope = _factory.Services.CreateScope();
        var vetContext = scope.ServiceProvider.GetRequiredService<global::Veterinary.Infrastructure.Persistence.VeterinaryDbContext>();

        var appointmentId = Guid.NewGuid();
        var petId = Guid.NewGuid();
        var appointment = Appointment.Create(appointmentId, Guid.NewGuid(), petId, Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1), 30, "Check").Value;
        appointment.Confirm();
        appointment.Start();
        vetContext.Appointments.Add(appointment);
        await vetContext.SaveChangesAsync();

        var createResponse = await client.PostAsync($"/api/v1/appointments/{appointmentId}/records", null);
        createResponse.IsSuccessStatusCode.Should().BeTrue();
        var recordId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var patchResponse = await client.PatchAsJsonAsync($"/api/v1/medical-records/{recordId}/anamnesis", new { Anamnesis = "Vomiting" });
        patchResponse.IsSuccessStatusCode.Should().BeTrue();

        var getResponse = await client.GetAsync($"/api/v1/medical-records/{recordId}");
        getResponse.IsSuccessStatusCode.Should().BeTrue();
        var detail = await getResponse.Content.ReadFromJsonAsync<MedicalRecordDto>();
        detail!.Anamnesis.Should().Be("Vomiting");

        var finalizeResponse = await client.PostAsync($"/api/v1/medical-records/{recordId}/finalize", null);
        finalizeResponse.IsSuccessStatusCode.Should().BeTrue();
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

        var user = await userManager.FindByEmailAsync("vet-records@sysvet.com");
        if (user is null)
        {
            user = new Core.Infrastructure.Identity.AppUser { UserName = "vet-records@sysvet.com", Email = "vet-records@sysvet.com", TenantId = Guid.NewGuid() };
            await userManager.CreateAsync(user, "Password123!");
            await userManager.AddToRoleAsync(user, "Admin");
        }

        var httpClient = _factory.CreateClient();
        var loginResponse = await httpClient.PostAsJsonAsync("/api/v1/auth/login", new { Email = "vet-records@sysvet.com", Password = "Password123!" });
        var tokens = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);
        return httpClient;
    }

    private sealed record LoginResponse(string AccessToken, string RefreshToken);
}
