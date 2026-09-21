using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Inventory.Application.Products.Commands;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Petshop.Application.GroomingServices;
using Petshop.Domain.Enums;
using Xunit;

namespace API.IntegrationTests.Petshop;

[Collection("IntegrationTests")]
public class GroomingEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public GroomingEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var coreContext = scope.ServiceProvider.GetRequiredService<Core.Infrastructure.Persistence.CoreDbContext>();
        await coreContext.Database.EnsureDeletedAsync();
        await coreContext.Database.MigrateAsync();
        await IntegrationTestDatabaseHelper.MigrateModuleDatabasesAsync(scope);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Core.Infrastructure.Identity.AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        var email = $"grooming-{Guid.NewGuid():N}@sysvet.com";
        var user = new Core.Infrastructure.Identity.AppUser { UserName = email, Email = email, TenantId = Guid.NewGuid() };
        await userManager.CreateAsync(user, "Password123!");
        await userManager.AddToRoleAsync(user, "Admin");

        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = "Password123!" });
        var token = (await login.Content.ReadFromJsonAsync<LoginResponseDto>())!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task CompleteGrooming_DebitsConfiguredSupplies()
    {
        var client = await CreateAuthenticatedClientAsync();
        var suffix = Guid.NewGuid().ToString()[..8];
        var groomerId = Guid.NewGuid();
        var tutorId = Guid.NewGuid();
        var petId = Guid.NewGuid();

        var productResponse = await client.PostAsJsonAsync("/api/v1/inventory/products", new RegisterProductCommand(
            "Shampoo " + suffix,
            "",
            "SH-" + suffix,
            "789" + suffix,
            "UN",
            0m,
            ProductCategory.Hygiene,
            "33051000",
            null,
            0,
            null,
            null));
        productResponse.EnsureSuccessStatusCode();
        var productId = await productResponse.Content.ReadFromJsonAsync<Guid>();

        var stockIn = await client.PostAsJsonAsync("/api/v1/inventory/stock/movements", new
        {
            productId,
            type = MovementType.In,
            quantity = 5m,
            reason = "Opening"
        });
        stockIn.EnsureSuccessStatusCode();

        var serviceId = Guid.NewGuid();
        var upsertService = await client.PutAsJsonAsync("/api/v1/grooming-services", new UpsertGroomingServiceCommand(
            serviceId,
            "Banho completo",
            GroomingServiceType.Banho,
            60,
            "Banho",
            true,
            [new GroomingServiceSupplyLineDto(productId, 1m)],
            Guid.Empty));
        upsertService.EnsureSuccessStatusCode();

        var services = await client.GetFromJsonAsync<List<GroomingServiceDto>>("/api/v1/grooming-services");
        services!.Should().ContainSingle(s => s.Id == serviceId && s.DefaultSupplies.Count == 1);

        var day = DateTimeOffset.UtcNow.AddDays(1).Date;
        var slotDate = new DateTimeOffset(day, TimeSpan.Zero);
        var defineSlots = await client.PostAsJsonAsync("/api/v1/grooming-slots/availability", new
        {
            groomerId,
            date = slotDate,
            dayStart = "08:00:00",
            dayEnd = "18:00:00",
            slotDurationMinutes = 60
        });
        defineSlots.EnsureSuccessStatusCode();

        var appointmentId = Guid.NewGuid();
        var appointmentTime = slotDate.AddHours(9);
        var schedule = await client.PostAsJsonAsync("/api/v1/grooming-appointments", new
        {
            id = appointmentId,
            tutorId,
            petId,
            groomerId,
            groomingServiceId = serviceId,
            date = appointmentTime,
            durationInMinutes = 60,
            notes = "Pelagem sensível"
        });
        schedule.EnsureSuccessStatusCode();

        var recordBefore = await client.GetFromJsonAsync<GroomingRecordResponse>($"/api/v1/grooming-appointments/{appointmentId}/record");
        recordBefore!.SupplyLines.Should().ContainSingle(l => l.ProductId == productId);

        (await client.PostAsync($"/api/v1/grooming-appointments/{appointmentId}/confirm", null)).EnsureSuccessStatusCode();
        (await client.PostAsync($"/api/v1/grooming-appointments/{appointmentId}/start", null)).EnsureSuccessStatusCode();
        (await client.PostAsync($"/api/v1/grooming-appointments/{appointmentId}/complete", null)).EnsureSuccessStatusCode();

        using var scope = _factory.Services.CreateScope();
        var inventory = scope.ServiceProvider.GetRequiredService<global::Inventory.Infrastructure.Persistence.InventoryDbContext>();
        var movements = await inventory.StockMovements
            .IgnoreQueryFilters()
            .Where(m => m.CorrelationId == appointmentId)
            .ToListAsync();

        movements.Should().ContainSingle(m => m.Reason.StartsWith("Grooming", StringComparison.Ordinal));
        movements.Single().Quantity.Should().Be(1m);
    }

    private sealed class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
    }

    private sealed class GroomingRecordResponse
    {
        public IReadOnlyList<GroomingServiceSupplyLineDto> SupplyLines { get; set; } = Array.Empty<GroomingServiceSupplyLineDto>();
    }
}
