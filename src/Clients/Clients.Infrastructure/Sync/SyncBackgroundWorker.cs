using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Clients.Infrastructure.Sync;

/// <summary>
/// Worker de sincronização em segundo plano (roda nos clientes PWA e MAUI).
/// Processa a fila local FIFO, envia para a API e aplica pull.
/// </summary>
public class SyncBackgroundWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISyncConnectivity _connectivity;
    private readonly ILogger<SyncBackgroundWorker> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(30);
    private readonly SyncWakeSignal _wakeSignal;

    /// <summary>Creates the worker.</summary>
    public SyncBackgroundWorker(
        IServiceProvider serviceProvider,
        ISyncConnectivity connectivity,
        SyncWakeSignal wakeSignal,
        ILogger<SyncBackgroundWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _connectivity = connectivity;
        _wakeSignal = wakeSignal;
        _logger = logger;
        _connectivity.OnlineStateChanged += (_, _) => _wakeSignal.RequestSync();
    }

    /// <summary>Requests an immediate sync cycle when online.</summary>
    public void RequestSync() => _wakeSignal.RequestSync();

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SyncBackgroundWorker iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_connectivity.IsOnline)
                {
                    _connectivity.SetSyncing(true);
                    await ProcessSyncCycleAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado durante a sincronização.");
            }
            finally
            {
                _connectivity.SetSyncing(false);
            }

            var delayTask = Task.Delay(_pollingInterval, stoppingToken);
            var wakeTask = _wakeSignal.WakeSemaphore.WaitAsync(stoppingToken);
            await Task.WhenAny(delayTask, wakeTask);
        }
    }

    /// <summary>
    /// Runs one push/pull cycle (outbox then pull). Exposed for PoC integration tests (roadmap 3.6).
    /// </summary>
    internal async Task ProcessSyncCycleAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OfflineDbContext>();
        var syncClient = scope.ServiceProvider.GetRequiredService<ISyncHttpClient>();
        var pullApplier = scope.ServiceProvider.GetRequiredService<OfflineSyncPullApplier>();

        await PushOutboxAsync(dbContext, syncClient, cancellationToken);
        await PullRemoteChangesAsync(dbContext, syncClient, pullApplier, cancellationToken);

        stopwatch.Stop();
        var pendingCount = await dbContext.OutboxMessages
            .CountAsync(m => m.ProcessedAt == null && m.Error == null, cancellationToken);
        var errorCount = await dbContext.OutboxMessages.CountAsync(m => m.Error != null, cancellationToken);
        _logger.LogInformation(
            "Sync cycle completed in {ElapsedMs}ms. Pending={PendingCount} Errors={ErrorCount}",
            stopwatch.ElapsedMilliseconds,
            pendingCount,
            errorCount);
    }

    private async Task PushOutboxAsync(OfflineDbContext dbContext, ISyncHttpClient syncClient, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        // SQLite (client) does not translate all DateTimeOffset comparisons; filter retry window in memory.
        var pendingMessages = (await dbContext.OutboxMessages
                .Where(m => m.ProcessedAt == null && m.Error == null)
                .ToListAsync(cancellationToken))
            .Where(m => m.NextRetryAt == null || m.NextRetryAt <= now)
            .OrderBy(m => m.CreatedAt)
            .ThenBy(m => m.Id)
            .Take(50)
            .ToList();

        if (pendingMessages.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Processando {Count} mensagens da fila Outbox...", pendingMessages.Count);

        var pushResult = await syncClient.PushAsync(pendingMessages, cancellationToken);
        if (pushResult is null)
        {
            foreach (var msg in pendingMessages)
            {
                ScheduleRetry(msg);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogWarning("Falha de rede ao enviar lote de sincronização.");
            return;
        }

        foreach (var processedId in pushResult.ProcessedIds)
        {
            var msg = pendingMessages.FirstOrDefault(m => m.Id == processedId);
            if (msg is not null)
            {
                msg.ProcessedAt = DateTimeOffset.UtcNow;
            }
        }

        if (pushResult.FailedMessageId is Guid failedId)
        {
            var failed = pendingMessages.FirstOrDefault(m => m.Id == failedId);
            if (failed is not null)
            {
                if (pushResult.IsPermanentFailure)
                {
                    failed.Error = $"{pushResult.ErrorCode}: {pushResult.ErrorMessage}";
                }
                else
                {
                    ScheduleRetry(failed);
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void ScheduleRetry(OutboxMessage message)
    {
        message.AttemptCount++;
        var delaySeconds = Math.Min(Math.Pow(2, message.AttemptCount), 300);
        message.NextRetryAt = DateTimeOffset.UtcNow.AddSeconds(delaySeconds);
    }

    private async Task PullRemoteChangesAsync(
        OfflineDbContext dbContext,
        ISyncHttpClient syncClient,
        OfflineSyncPullApplier pullApplier,
        CancellationToken cancellationToken)
    {
        var state = await dbContext.SyncState.FindAsync([1], cancellationToken)
                    ?? dbContext.SyncState.Add(new SyncState()).Entity;
        var since = state.LastPullAt;

        var hasMore = true;
        while (hasMore && _connectivity.IsOnline)
        {
            var page = await syncClient.PullAsync(since, 100, cancellationToken);
            if (page is null)
            {
                _logger.LogWarning("Falha ao puxar alterações remotas.");
                break;
            }

            if (IsEmptyPullPage(page))
            {
                break;
            }

            await pullApplier.ApplyAsync(page, cancellationToken);
            since = page.NextSince;
            hasMore = page.HasMore;
        }
    }

    private static bool IsEmptyPullPage(ClientPullChangesResult page) =>
        page.Tutors.Count == 0
        && page.Pets.Count == 0
        && page.Appointments.Count == 0
        && page.ScheduleSlots.Count == 0
        && page.MedicalRecords.Count == 0
        && page.PrescriptionTemplates.Count == 0
        && page.IssuedPrescriptions.Count == 0
        && page.ClinicalExams.Count == 0
        && page.ClinicalAttachments.Count == 0
        && page.VaccineProtocols.Count == 0
        && page.VaccineDoses.Count == 0
        && page.ClinicalQuotes.Count == 0
        && page.WardUnits.Count == 0
        && page.Hospitalizations.Count == 0
        && page.InventoryProducts.Count == 0
        && page.InventoryProductLots.Count == 0
        && page.InventorySuppliers.Count == 0
        && page.InventoryStockMovements.Count == 0
        && page.SalesCashRegisters.Count == 0
        && page.SalesOrders.Count == 0
        && page.SalesCommissionRules.Count == 0;
}
