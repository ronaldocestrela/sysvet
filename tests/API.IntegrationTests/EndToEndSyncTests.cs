using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Clients.Infrastructure;
using Clients.Infrastructure.Sync;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace API.IntegrationTests;

[Collection("IntegrationTests")]
public class EndToEndSyncTests
{
    private readonly WebApplicationFactory<Program> _factory;

    public EndToEndSyncTests()
    {
        var dbName = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), $"e2e_sync_{Guid.NewGuid()}.db");
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<Core.Infrastructure.Persistence.CoreDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<Core.Infrastructure.Persistence.CoreDbContext>(options =>
                {
                    options.UseSqlite($"Data Source={dbName}");
                });
            });
        });
    }

    private async Task SeedUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Core.Infrastructure.Persistence.CoreDbContext>();
        await context.Database.EnsureCreatedAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Core.Infrastructure.Identity.AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole("Admin"));

        var user = await userManager.FindByEmailAsync("syncadmin@sysvet.com");
        if (user == null)
        {
            user = new Core.Infrastructure.Identity.AppUser { UserName = "syncadmin@sysvet.com", Email = "syncadmin@sysvet.com", TenantId = Guid.NewGuid() };
            var result = await userManager.CreateAsync(user, "Password123!");
            if (!result.Succeeded) throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }

    [Fact]
    public async Task Create_Tutor_Offline_And_Sync_To_Cloud()
    {
        // 1. Arrange - Setup Offline Client Database
        var offlineDbOptions = new DbContextOptionsBuilder<OfflineDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        using var offlineDb = new OfflineDbContext(offlineDbOptions);
        await offlineDb.Database.OpenConnectionAsync(); // keep connection open for in-memory sqlite
        await offlineDb.Database.EnsureCreatedAsync();

        // 2. Act - User creates a Tutor while Offline
        var cpfStr = "12345678909";
        var tutor = Tutor.Create(
            "Tutor E2E Test",
            Email.Create("e2e@test.com").Value,
            Cpf.Create(cpfStr).Value,
            Phone.Create("11988887777").Value
        ).Value;

        offlineDb.Tutors.Add(tutor);
        
        var stopwatch = Stopwatch.StartNew();
        await offlineDb.SaveChangesAsync(); // Saves to local DB and intercepts OutboxMessage
        
        // Assert offline behavior worked
        var outboxMessages = await offlineDb.OutboxMessages.ToListAsync();
        outboxMessages.Should().HaveCount(1);
        outboxMessages.First().Type.Should().Be("RegisterTutorCommand");

        // 3. Act - User reconnects to Internet (SyncBackgroundWorker triggers)
        // We simulate the SyncBackgroundWorker logic by reading Outbox and calling the API
        var pendingMessagesRaw = await offlineDb.OutboxMessages.ToListAsync();
        var pendingMessages = pendingMessagesRaw.OrderBy(m => m.CreatedAt).ToList();
        
        // Create an HTTP Client that points to our test API server
        var httpClient = _factory.CreateClient();
        
        // 2.5. Authenticate Client
        await SeedUserAsync();
        var loginResponse = await httpClient.PostAsJsonAsync("/api/v1/auth/login", new { Email = "syncadmin@sysvet.com", Password = "Password123!" });
        loginResponse.EnsureSuccessStatusCode();
        var tokenInfo = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenInfo!.AccessToken);

        // Push to API
        var pushResponse = await httpClient.PostAsJsonAsync("/api/v1/sync/push", pendingMessages);
        
        stopwatch.Stop();

        // Assert push was successful
        // Assert push was successful
        if (!pushResponse.IsSuccessStatusCode)
        {
            var err = await pushResponse.Content.ReadAsStringAsync();
            throw new Exception($"Push failed with status {pushResponse.StatusCode} and body {err}");
        }

        // After successful push, the client cleans up the outbox
        offlineDb.OutboxMessages.RemoveRange(pendingMessages);
        await offlineDb.SaveChangesAsync();

        // 4. Assert - Verify Data on the Server
        // We will call the standard GET endpoint to check if the Tutor was successfully persisted and rules applied
        var getResponse = await httpClient.GetAsync("/api/v1/tutors");
        getResponse.IsSuccessStatusCode.Should().BeTrue();
        
        var content = await getResponse.Content.ReadAsStringAsync();
        // Just verify if the CPF or Name is present in the results, ensuring it was saved in the central db
        content.Should().Contain(cpfStr);
        content.Should().Contain("Tutor E2E Test");

        // 5. Act - Pull changes back to client
        var lastSync = DateTimeOffset.UtcNow.AddMinutes(-5); // Simulating last sync time
        var pullResponse = await httpClient.GetAsync($"/api/v1/sync/pull?since={Uri.EscapeDataString(lastSync.ToString("O"))}");
        pullResponse.IsSuccessStatusCode.Should().BeTrue();

        var pullContent = await pullResponse.Content.ReadAsStringAsync();
        pullContent.Should().Contain(cpfStr); // The recently synced tutor should be returned in the pull

        // Documenting Metrics in the test output implicitly
        Console.WriteLine($"[METRICS] Sync time (Offline Save -> API Push): {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"[METRICS] Outbox messages synced: {pendingMessages.Count}");
    }
}
