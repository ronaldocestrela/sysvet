using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Automations.Application.Abstractions;
using Automations.Application.Campaigns.Commands;
using Automations.Application.Campaigns.Dtos;
using Automations.Domain.Enums;
using MediatR;
using Automations.Infrastructure.Persistence;
using Automations.Infrastructure.Campaigns;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using Core.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Enums;
using Veterinary.Infrastructure.Persistence;
using Xunit;

namespace API.IntegrationTests.Automations;

[Collection("IntegrationTests")]
public class CampaignNpsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly WebApplicationFactory<Program> _factory;

    public CampaignNpsIntegrationTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Campaign_EnqueuedForInactiveSegment_WhenLastVisitOlderThan90Days()
    {
        _ = await CreateAuthenticatedClientAsync();
        await using var scope = _factory.Services.CreateAsyncScope();
        SetTenant(scope);
        var core = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
        var vet = scope.ServiceProvider.GetRequiredService<VeterinaryDbContext>();
        var automations = scope.ServiceProvider.GetRequiredService<AutomationsDbContext>();

        await EnsureTemplatesAsync(automations);

        var tutor = Tutor.Create(
            "Inativo Tutor",
            Email.Create($"inactive-{Guid.NewGuid():N}@test.com").Value,
            Cpf.Create("39053344705").Value,
            Phone.Create("11988887777").Value).Value;
        core.Tutors.Add(tutor);
        var pet = Pet.Create("Luna", PetSpecies.Dog, "SRD", PetSex.Female, tutor.Id).Value;
        core.Pets.Add(pet);
        await core.SaveChangesAsync();

        var appointment = Appointment.RestoreFromSync(
            Guid.NewGuid(),
            tutor.Id,
            pet.Id,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(-120),
            30,
            "Consulta",
            AppointmentStatus.Completed,
            DateTimeOffset.UtcNow.AddDays(-100));
        await vet.Appointments.AddAsync(appointment);
        await vet.SaveChangesAsync();

        var campaign = global::Automations.Domain.Entities.Campaign.Create(
            "Reativação",
            CampaignSegmentKind.Inactive90Days,
            inactiveDays: 90,
            cooldownDays: 90).Value;
        automations.Campaigns.Add(campaign);
        await automations.SaveChangesAsync();

        var launchHandler = scope.ServiceProvider
            .GetRequiredService<IRequestHandler<LaunchCampaignCommand, Core.Domain.Result<CampaignRunDto>>>();
        var launchResult = await launchHandler.Handle(new LaunchCampaignCommand(campaign.Id), CancellationToken.None);
        launchResult.IsSuccess.Should().BeTrue(launchResult.Error?.Message ?? "launch failed");
        launchResult.Value!.AudienceCount.Should().BeGreaterThan(0, "inactive segment should match tutor with old visit");
        launchResult.Value!.EnqueuedCount.Should().BeGreaterThan(0);

        var jobs = await automations.MessageJobs
            .Where(j => j.TemplateCode == "campaign.inactive" && j.PayloadJson.Contains(tutor.Id.ToString("N")))
            .ToListAsync();
        jobs.Should().NotBeEmpty();
    }

    [Fact]
    public async Task NpsResponse_RecordedAndReportable()
    {
        var client = await CreateAuthenticatedClientAsync();
        await using var scope = _factory.Services.CreateAsyncScope();
        SetTenant(scope);
        var core = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
        var vet = scope.ServiceProvider.GetRequiredService<VeterinaryDbContext>();
        var automations = scope.ServiceProvider.GetRequiredService<AutomationsDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<INpsSurveyTokenService>();

        await EnsureTemplatesAsync(automations);

        var campaign = global::Automations.Domain.Entities.Campaign.Create("NPS pós", CampaignSegmentKind.PostAppointment).Value;
        campaign.Update("NPS pós", "nps.request", 90, 90, CampaignStatus.Active);
        automations.Campaigns.Add(campaign);
        await automations.SaveChangesAsync();

        var tutor = Tutor.Create(
            "NPS Tutor",
            Email.Create($"nps-{Guid.NewGuid():N}@test.com").Value,
            Cpf.Create("15350946056").Value,
            Phone.Create("11977776666").Value).Value;
        core.Tutors.Add(tutor);
        var pet = Pet.Create("Thor", PetSpecies.Dog, "SRD", PetSex.Male, tutor.Id).Value;
        core.Pets.Add(pet);
        await core.SaveChangesAsync();

        var appointment = Appointment.RestoreFromSync(
            Guid.NewGuid(),
            tutor.Id,
            pet.Id,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddHours(-2),
            30,
            "Consulta",
            AppointmentStatus.Completed,
            DateTimeOffset.UtcNow);
        await vet.Appointments.AddAsync(appointment);
        await vet.SaveChangesAsync();

        var scan = scope.ServiceProvider.GetRequiredService<CampaignScanService>();
        (await scan.ScanAsync(CancellationToken.None)).Should().BeGreaterThan(0);

        var invite = await automations.NpsInvites.FirstAsync(i => i.SourceId == appointment.Id);
        var token = tokenService.CreateToken(
            IntegrationTestDatabaseHelper.SingleTenantId,
            invite.Id,
            invite.ExpiresAt);

        var publicClient = _factory.CreateClient();
        var submit = await publicClient.PostAsJsonAsync(
            $"/api/v1/public/nps/{token}",
            new { score = 10, comment = "Excelente" });
        submit.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var reportResponse = await client.GetAsync("/api/v1/automations/nps/report");
        reportResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var reportJson = await reportResponse.Content.ReadAsStringAsync();
        reportJson.Should().Contain("totalResponses");
        reportJson.Should().Contain("Excelente");
    }

    [Fact]
    public async Task NpsInvite_NotEnqueued_WhenMarketingOptedOut()
    {
        var client = await CreateAuthenticatedClientAsync();
        await using var scope = _factory.Services.CreateAsyncScope();
        SetTenant(scope);
        var core = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
        var vet = scope.ServiceProvider.GetRequiredService<VeterinaryDbContext>();
        var automations = scope.ServiceProvider.GetRequiredService<AutomationsDbContext>();

        await EnsureTemplatesAsync(automations);

        var campaign = global::Automations.Domain.Entities.Campaign.Create("NPS", CampaignSegmentKind.PostAppointment).Value;
        campaign.Update("NPS", "nps.request", 90, 90, CampaignStatus.Active);
        automations.Campaigns.Add(campaign);
        await automations.SaveChangesAsync();

        var tutor = Tutor.Create(
            "Opt Marketing",
            Email.Create($"mkt-{Guid.NewGuid():N}@test.com").Value,
            Cpf.Create("11144477735").Value,
            Phone.Create("11966665555").Value).Value;
        core.Tutors.Add(tutor);
        var pet = Pet.Create("Mel", PetSpecies.Cat, "SRD", PetSex.Female, tutor.Id).Value;
        core.Pets.Add(pet);
        await core.SaveChangesAsync();

        var prefResponse = await client.PutAsJsonAsync(
            $"/api/v1/automations/tutors/{tutor.Id}/preferences",
            new { whatsAppEnabled = true, emailEnabled = true, marketingEnabled = false });
        prefResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var appointment = Appointment.RestoreFromSync(
            Guid.NewGuid(),
            tutor.Id,
            pet.Id,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddHours(-1),
            30,
            "Consulta",
            AppointmentStatus.Completed,
            DateTimeOffset.UtcNow);
        await vet.Appointments.AddAsync(appointment);
        await vet.SaveChangesAsync();

        var scan = scope.ServiceProvider.GetRequiredService<CampaignScanService>();
        await scan.ScanAsync(CancellationToken.None);

        var jobs = await automations.MessageJobs
            .Where(j => j.TemplateCode == "nps.request" && j.PayloadJson.Contains(tutor.Id.ToString("N")))
            .ToListAsync();
        jobs.Should().BeEmpty();
    }

    private static async Task EnsureTemplatesAsync(AutomationsDbContext automations)
    {
        async Task Ensure(string code, MessageChannel channel, string body)
        {
            if (await automations.MessageTemplates.AnyAsync(t => t.Code == code && t.Channel == channel))
            {
                return;
            }

            automations.MessageTemplates.Add(
                global::Automations.Domain.Entities.MessageTemplate.Create(code, channel, body).Value);
        }

        await Ensure("campaign.inactive", MessageChannel.WhatsApp, "Olá {{TutorName}}");
        await Ensure("campaign.inactive", MessageChannel.Email, "Olá {{TutorName}}");
        await Ensure("nps.request", MessageChannel.WhatsApp, "{{SurveyUrl}}");
        await Ensure("nps.request", MessageChannel.Email, "{{SurveyUrl}}");
        await automations.SaveChangesAsync();
    }

    private static void SetTenant(IServiceScope scope)
    {
        var tenantContext = scope.ServiceProvider.GetRequiredService<Core.Domain.ITenantContext>();
        var tenantId = IntegrationTestDatabaseHelper.SingleTenantId;
        tenantContext.TenantId = tenantId;
        tenantContext.SchemaName = "dbo";
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

        var email = $"campaign-{Guid.NewGuid():N}@sysvet.com";
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
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>(JsonOptions);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse!.AccessToken);
        return client;
    }

    private sealed record LoginResponseDto(string AccessToken);
}
