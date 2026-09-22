using System.Net.Http.Headers;
using System.Net.Http.Json;
using Automations.Domain.Enums;
using Automations.Infrastructure.Persistence;
using Automations.Infrastructure.Reminders;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using Core.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Veterinary.Domain.Entities;
using Veterinary.Infrastructure.Persistence;
using Xunit;

namespace API.IntegrationTests.Automations;

[Collection("IntegrationTests")]
public class VaccineReminderIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public VaccineReminderIntegrationTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task VaccineReminder_EnqueuedSevenDaysBefore_WhenUpcoming()
    {
        var client = await CreateAuthenticatedClientAsync();
        await using var scope = _factory.Services.CreateAsyncScope();

        var core = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
        var vet = scope.ServiceProvider.GetRequiredService<VeterinaryDbContext>();
        var automations = scope.ServiceProvider.GetRequiredService<AutomationsDbContext>();

        var tutor = Tutor.Create(
            "Vacina Tutor",
            Email.Create($"vacina-{Guid.NewGuid():N}@test.com").Value,
            Cpf.Create("39053344705").Value,
            Phone.Create("11988887777").Value).Value;
        core.Tutors.Add(tutor);

        var pet = Pet.Create("Rex", PetSpecies.Dog, "SRD", PetSex.Male, tutor.Id).Value;
        core.Pets.Add(pet);
        await core.SaveChangesAsync();

        var tz = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "E. South America Standard Time" : "America/Sao_Paulo");
        var localToday = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz).DateTime);
        var dueLocal = localToday.AddDays(7);
        var dueUtc = new DateTimeOffset(dueLocal.ToDateTime(new TimeOnly(12, 0), DateTimeKind.Unspecified), tz.GetUtcOffset(dueLocal.ToDateTime(new TimeOnly(12, 0))));

        var dose = VaccineDose.Create(
            Guid.NewGuid(),
            pet.Id,
            "V8",
            "Lote1",
            DateTimeOffset.UtcNow.AddMonths(-1),
            dueUtc).Value;
        await vet.VaccineDoses.AddAsync(dose);
        await vet.SaveChangesAsync();

        var scan = scope.ServiceProvider.GetRequiredService<ReminderScanService>();
        var enqueued = await scan.ScanAsync(CancellationToken.None);
        enqueued.Should().BeGreaterThan(0);

        var idempotencyPrefix = $"reminder:vaccine:{dose.Id:N}:d-7";
        var jobs = await automations.MessageJobs
            .Where(j => j.TemplateCode == "reminder.vaccine" && j.IdempotencyKey.StartsWith(idempotencyPrefix))
            .ToListAsync();
        jobs.Should().NotBeEmpty();
        jobs.Should().Contain(j => j.Channel == MessageChannel.WhatsApp || j.Channel == MessageChannel.Email);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        using var scope = _factory.Services.CreateAsyncScope();
        var coreContext = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
        await coreContext.Database.EnsureDeletedAsync();
        await coreContext.Database.MigrateAsync();
        await IntegrationTestDatabaseHelper.MigrateModuleDatabasesAsync(scope);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Core.Infrastructure.Identity.AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        var email = $"vacina-admin-{Guid.NewGuid():N}@sysvet.com";
        var user = new Core.Infrastructure.Identity.AppUser
        {
            UserName = email,
            Email = email,
            TenantId = IntegrationTestDatabaseHelper.SingleTenantId
        };
        await userManager.CreateAsync(user, "Password123!");
        await userManager.AddToRoleAsync(user, "Admin");

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = "Password123!" });
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse!.AccessToken);
        return client;
    }

    private sealed record LoginResponseDto(string AccessToken);
}
