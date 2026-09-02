using System;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Core.Infrastructure.Identity;
using System.Net.Http.Headers;

namespace API.IntegrationTests.Veterinary;

[Collection("IntegrationTests")]
public class HospitalizationEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HospitalizationEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private async Task<System.Net.Http.HttpClient> CreateAuthenticatedClientAsync()
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
        
        // Define LoginResponse locally or map it manually to avoid missing reference
        var loginResult = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var token = loginResult.GetProperty("accessToken").GetString();
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task HospitalizationLifecycle_ShouldSucceed()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();
        var petId = Guid.NewGuid();
        var veterinarianId = Guid.NewGuid();

        // 1. Admit Pet
        var admitCommand = new
        {
            PetId = petId,
            VeterinarianId = veterinarianId,
            Reason = "Severe vomiting"
        };
        var admitResponse = await client.PostAsJsonAsync("/api/v1/hospitalizations", admitCommand);
        admitResponse.EnsureSuccessStatusCode();
        var hospitalizationId = await admitResponse.Content.ReadFromJsonAsync<Guid>();
        hospitalizationId.Should().NotBeEmpty();

        // 2. Execute Prescription
        var execCommand = new
        {
            HospitalizationId = hospitalizationId,
            MedicationName = "Ondansetron",
            Dose = "4mg",
            Notes = "IV injection",
            ExecutedBy = Guid.NewGuid()
        };
        var execResponse = await client.PostAsJsonAsync($"/api/v1/hospitalizations/{hospitalizationId}/prescriptions/execute", execCommand);
        execResponse.EnsureSuccessStatusCode();
        var executionId = await execResponse.Content.ReadFromJsonAsync<Guid>();
        executionId.Should().NotBeEmpty();

        // 3. Discharge Pet
        var dischargeResponse = await client.PostAsync($"/api/v1/hospitalizations/{hospitalizationId}/discharge", null);
        dischargeResponse.EnsureSuccessStatusCode();
        dischargeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 4. Try to execute prescription after discharge (should fail)
        var failExecResponse = await client.PostAsJsonAsync($"/api/v1/hospitalizations/{hospitalizationId}/prescriptions/execute", execCommand);
        failExecResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
