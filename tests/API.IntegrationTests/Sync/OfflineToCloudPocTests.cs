using System.Net.Http.Json;
using Clients.Infrastructure.Sync;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace API.IntegrationTests.Sync;

/// <summary>
/// PoC E2E (roadmap 3.6): SQLite local → <see cref="SyncBackgroundWorker"/> → API nuvem.
/// </summary>
[Collection("IntegrationTests")]
public class OfflineToCloudPocTests
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    public OfflineToCloudPocTests(ITestOutputHelper output)
    {
        _output = output;
        var dbName = Path.Combine(Directory.GetCurrentDirectory(), $"poc_sync_{Guid.NewGuid()}.db");
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(DbContextOptions<Core.Infrastructure.Persistence.CoreDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<Core.Infrastructure.Persistence.CoreDbContext>(options =>
                {
                    options.UseSqlite($"Data Source={dbName}");
                });
            });
        });
    }

    [Fact]
    public async Task OfflineCreateTutor_WorkerCycle_ShouldSyncToCloudWithZeroPending()
    {
        var apiClient = await SyncPocHarness.CreateAuthenticatedClientAsync(_factory, SeedUserAsync);
        await using var harness = await SyncPocHarness.CreateAsync(apiClient);

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var cpf = "52998224725";
        var tutor = Tutor.Create(
            $"PoC Tutor {suffix}",
            Email.Create($"poc-{suffix}@test.com").Value,
            Cpf.Create(cpf).Value,
            Phone.Create("11988887777").Value).Value;

        harness.OfflineDb.Tutors.Add(tutor);
        await harness.OfflineDb.SaveChangesAsync();

        (await harness.OfflineDb.OutboxMessages.CountAsync()).Should().Be(1);

        var metrics = await harness.RunSyncCycleAsync();
        _output.WriteLine(metrics.ToLogLine());

        metrics.ElapsedMs.Should().BeGreaterThan(0);
        metrics.PendingCount.Should().Be(0);
        metrics.ErrorCount.Should().Be(0);
        metrics.ProcessedCount.Should().Be(1);

        var getResponse = await harness.ApiClient.GetAsync("/api/v1/tutors");
        getResponse.EnsureSuccessStatusCode();
        var body = await getResponse.Content.ReadAsStringAsync();
        body.Should().Contain(cpf);
        body.Should().Contain($"PoC Tutor {suffix}");
    }

    [Fact]
    public async Task EmptySecondClient_WorkerCycle_ShouldPullTutorWithoutOutbox()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var cpf = "39053344705";
        var tutorId = Guid.NewGuid();

        var apiClient = await SyncPocHarness.CreateAuthenticatedClientAsync(_factory, SeedUserAsync);
        await using (var seedHarness = await SyncPocHarness.CreateAsync(apiClient))
        {
            var tutor = Tutor.Create(
                $"Pull Tutor {suffix}",
                Email.Create($"pull-{suffix}@test.com").Value,
                Cpf.Create(cpf).Value,
                Phone.Create("11977776666").Value,
                tutorId).Value;
            seedHarness.OfflineDb.Tutors.Add(tutor);
            await seedHarness.OfflineDb.SaveChangesAsync();
            var seedMetrics = await seedHarness.RunSyncCycleAsync();
            seedMetrics.PendingCount.Should().Be(0);
        }

        await using var secondClient = await SyncPocHarness.CreateAsync(apiClient);

        (await secondClient.OfflineDb.Tutors.CountAsync()).Should().Be(0);

        var metrics = await secondClient.RunSyncCycleAsync();
        _output.WriteLine(metrics.ToLogLine());

        (await secondClient.OfflineDb.Tutors.CountAsync()).Should().Be(1);
        (await secondClient.OfflineDb.OutboxMessages.CountAsync()).Should().Be(0);
        metrics.ErrorCount.Should().Be(0);
    }

    [Fact]
    public async Task UnknownOutboxType_WorkerCycle_ShouldDeadLetterWithErrorMetric()
    {
        var apiClient = await SyncPocHarness.CreateAuthenticatedClientAsync(_factory, SeedUserAsync);
        await using var harness = await SyncPocHarness.CreateAsync(apiClient);

        var badMessage = new OutboxMessage
        {
            Type = "TotallyUnknownCommand",
            Payload = "{}",
            CreatedAt = DateTimeOffset.UtcNow
        };
        harness.OfflineDb.OutboxMessages.Add(badMessage);
        await harness.OfflineDb.SaveChangesAsync();

        var metrics = await harness.RunSyncCycleAsync();
        _output.WriteLine(metrics.ToLogLine());

        metrics.ErrorCount.Should().Be(1);
        metrics.PendingCount.Should().Be(0);
        metrics.ProcessedCount.Should().Be(0);

        var deadLetter = await harness.OfflineDb.OutboxMessages.SingleAsync();
        deadLetter.Error.Should().NotBeNullOrEmpty();
        deadLetter.Error.Should().Contain("Sync.UnknownCommandType");
    }

    private async Task SeedUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Core.Infrastructure.Persistence.CoreDbContext>();
        await context.Database.EnsureCreatedAsync();

        var vetContext = scope.ServiceProvider.GetRequiredService<global::Veterinary.Infrastructure.Persistence.VeterinaryDbContext>();
        await vetContext.Database.MigrateAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<
            Microsoft.AspNetCore.Identity.UserManager<Core.Infrastructure.Identity.AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<
            Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole("Admin"));
        }

        var user = await userManager.FindByEmailAsync("syncadmin@sysvet.com");
        if (user == null)
        {
            user = new Core.Infrastructure.Identity.AppUser
            {
                UserName = "syncadmin@sysvet.com",
                Email = "syncadmin@sysvet.com",
                TenantId = Guid.NewGuid()
            };
            var result = await userManager.CreateAsync(user, "Password123!");
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            await userManager.AddToRoleAsync(user, "Admin");
        }
    }
}
