using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Veterinary.Application.Quotes.Dtos;
using Veterinary.Domain.Entities;
using Xunit;

namespace API.IntegrationTests.Veterinary;

[Collection("IntegrationTests")]
public class ClinicalQuoteEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly WebApplicationFactory<Program> _factory;

    public ClinicalQuoteEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Quote_DraftToApproved_AppearsInPendingConversions()
    {
        var client = await CreateAuthenticatedClientAsync();
        var appointmentId = await SeedEligibleAppointmentAsync();

        var createResponse = await client.PostAsJsonAsync($"/api/v1/appointments/{appointmentId}/quotes", new { Notes = "Proposta clínica" });
        createResponse.IsSuccessStatusCode.Should().BeTrue();
        var quoteId = await ReadGuidAsync(createResponse.Content);

        var itemsResponse = await client.PutAsJsonAsync($"/api/v1/clinical-quotes/{quoteId}/items", new
        {
            Notes = "Proposta clínica",
            Items = new[]
            {
                new { Id = Guid.Empty, Description = "Consulta", Quantity = 1m, UnitPrice = 120m, Kind = "Service", ProductId = (Guid?)null, SortOrder = 0 }
            }
        });
        itemsResponse.IsSuccessStatusCode.Should().BeTrue();

        (await client.PostAsync($"/api/v1/clinical-quotes/{quoteId}/send", null)).IsSuccessStatusCode.Should().BeTrue();
        (await client.PostAsync($"/api/v1/clinical-quotes/{quoteId}/approve", null)).IsSuccessStatusCode.Should().BeTrue();

        var pendingResponse = await client.GetAsync("/api/v1/clinical-quotes/pending-conversions");
        pendingResponse.IsSuccessStatusCode.Should().BeTrue();
        var pending = JsonSerializer.Deserialize<List<PendingQuoteConversionDto>>(await pendingResponse.Content.ReadAsStringAsync(), JsonOptions);
        pending!.Should().ContainSingle(p => p.QuoteId == quoteId && p.TotalAmount == 120m);
    }

    private async Task<Guid> SeedEligibleAppointmentAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var vetContext = scope.ServiceProvider.GetRequiredService<global::Veterinary.Infrastructure.Persistence.VeterinaryDbContext>();
        var appointmentId = Guid.NewGuid();
        var appointment = Appointment.Create(appointmentId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1), 30, "Quote").Value;
        appointment.Confirm();
        appointment.Start();
        vetContext.Appointments.Add(appointment);
        await vetContext.SaveChangesAsync();
        return appointmentId;
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

        var user = await userManager.FindByEmailAsync("vet-quotes@sysvet.com");
        if (user is null)
        {
            user = new Core.Infrastructure.Identity.AppUser { UserName = "vet-quotes@sysvet.com", Email = "vet-quotes@sysvet.com", TenantId = IntegrationTestDatabaseHelper.SingleTenantId };
            await userManager.CreateAsync(user, "Password123!");
            await userManager.AddToRoleAsync(user, "Admin");
        }

        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = "vet-quotes@sysvet.com", Password = "Password123!" });
        var tokens = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens!.AccessToken);
        return client;
    }

    private sealed record LoginResponse(string AccessToken, string RefreshToken);

    private static async Task<Guid> ReadGuidAsync(HttpContent content)
    {
        var body = await content.ReadAsStringAsync();
        return Guid.Parse(body.Trim('"'));
    }
}
