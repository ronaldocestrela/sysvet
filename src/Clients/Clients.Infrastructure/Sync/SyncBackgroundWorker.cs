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
    private readonly SemaphoreSlim _wakeSignal = new(0, 1);

    /// <summary>Creates the worker.</summary>
    public SyncBackgroundWorker(
        IServiceProvider serviceProvider,
        ISyncConnectivity connectivity,
        ILogger<SyncBackgroundWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _connectivity = connectivity;
        _logger = logger;
        _connectivity.OnlineStateChanged += (_, _) =>
        {
            try
            {
                _wakeSignal.Release();
            }
            catch (SemaphoreFullException)
            {
                // Already signaled.
            }
        };
    }

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
            var wakeTask = _wakeSignal.WaitAsync(stoppingToken);
            await Task.WhenAny(delayTask, wakeTask);
        }
    }

    private async Task ProcessSyncCycleAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OfflineDbContext>();
        var syncClient = scope.ServiceProvider.GetRequiredService<ISyncHttpClient>();
        var pullApplier = scope.ServiceProvider.GetRequiredService<OfflineSyncPullApplier>();

        await PushOutboxAsync(dbContext, syncClient, cancellationToken);
        await PullRemoteChangesAsync(dbContext, syncClient, pullApplier, cancellationToken);
    }

    private async Task PushOutboxAsync(OfflineDbContext dbContext, ISyncHttpClient syncClient, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var pendingMessages = (await dbContext.OutboxMessages
            .Where(m => m.ProcessedAt == null && m.Error == null)
            .Where(m => m.NextRetryAt == null || m.NextRetryAt <= now)
            .ToListAsync(cancellationToken))
            .OrderBy(m => m.CreatedAt)
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

            if (page.Tutors.Count == 0 && page.Pets.Count == 0)
            {
                break;
            }

            await pullApplier.ApplyAsync(page, cancellationToken);
            since = page.NextSince;
            hasMore = page.HasMore;
        }
    }
}
