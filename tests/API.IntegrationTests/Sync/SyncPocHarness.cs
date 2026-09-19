using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Clients.Infrastructure;
using Clients.Infrastructure.Persistence;
using Clients.Infrastructure.Sync;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace API.IntegrationTests.Sync;

/// <summary>
/// Wires production client sync stack (worker, HTTP client, SQLite) against a test API base address.
/// </summary>
public sealed class SyncPocHarness : IAsyncDisposable
{
    private readonly OfflineDbContext _offlineDb;
    private readonly ServiceProvider _serviceProvider;
    private readonly SyncBackgroundWorker _worker;

    private SyncPocHarness(
        OfflineDbContext offlineDb,
        HttpClient apiClient,
        ServiceProvider serviceProvider,
        SyncBackgroundWorker worker)
    {
        _offlineDb = offlineDb;
        ApiClient = apiClient;
        _serviceProvider = serviceProvider;
        _worker = worker;
    }

    /// <summary>Local SQLite database used by this harness instance.</summary>
    public OfflineDbContext OfflineDb => _offlineDb;

    /// <summary>Authenticated HTTP client used by the sync worker (same instance for API assertions).</summary>
    public HttpClient ApiClient { get; }

    /// <summary>Creates an in-memory offline DB and sync worker bound to the authenticated HTTP client.</summary>
    public static async Task<SyncPocHarness> CreateAsync(HttpClient apiClient)
    {
        var offlineDbOptions = new DbContextOptionsBuilder<OfflineDbContext>()
            .UseSqlite("DataSource=:memory:")
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
            .Options;

        var offlineDb = new OfflineDbContext(offlineDbOptions, new NoOpSqliteFilePersistence());
        await offlineDb.Database.OpenConnectionAsync();
        await offlineDb.Database.EnsureCreatedAsync();

        var connectivity = new FakeSyncConnectivity();
        connectivity.SetOnline(true);

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
        services.AddSingleton(connectivity);
        services.AddSingleton<ISyncConnectivity>(sp => sp.GetRequiredService<FakeSyncConnectivity>());
        // Singleton so scope disposal in SyncBackgroundWorker does not dispose the shared test database.
        services.AddSingleton(offlineDb);
        services.AddSingleton(new OfflineSyncPullApplier(offlineDb));
        services.AddSingleton<ISyncHttpClient>(_ => new SyncHttpClient(apiClient));
        services.AddSingleton<SyncWakeSignal>();
        services.AddSingleton<SyncBackgroundWorker>();
        services.AddSingleton<IServiceProvider>(sp => sp);

        var serviceProvider = services.BuildServiceProvider();
        var worker = serviceProvider.GetRequiredService<SyncBackgroundWorker>();

        return new SyncPocHarness(offlineDb, apiClient, serviceProvider, worker);
    }

    /// <summary>Runs one deterministic sync cycle and returns PoC metrics.</summary>
    public async Task<SyncPocMetrics> RunSyncCycleAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        await _worker.ProcessSyncCycleAsync(cancellationToken);
        stopwatch.Stop();

        var pending = await _offlineDb.OutboxMessages
            .CountAsync(m => m.ProcessedAt == null && m.Error == null, cancellationToken);
        var errors = await _offlineDb.OutboxMessages.CountAsync(m => m.Error != null, cancellationToken);
        var processed = await _offlineDb.OutboxMessages.CountAsync(m => m.ProcessedAt != null, cancellationToken);

        return new SyncPocMetrics
        {
            ElapsedMs = stopwatch.ElapsedMilliseconds,
            PendingCount = pending,
            ErrorCount = errors,
            ProcessedCount = processed
        };
    }

    /// <summary>Authenticates against the test API and returns a client with Bearer token set.</summary>
    public static async Task<HttpClient> CreateAuthenticatedClientAsync(
        Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> factory,
        Func<Task> seedUserAsync)
    {
        await seedUserAsync();
        var httpClient = factory.CreateClient();
        var loginResponse = await httpClient.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { Email = "syncadmin@sysvet.com", Password = "Password123!" });
        loginResponse.EnsureSuccessStatusCode();
        var tokenInfo = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenInfo!.AccessToken);
        return httpClient;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _offlineDb.DisposeAsync();
        await _serviceProvider.DisposeAsync();
    }
}
