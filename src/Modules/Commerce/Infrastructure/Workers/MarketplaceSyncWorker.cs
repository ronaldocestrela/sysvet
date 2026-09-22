using Commerce.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Commerce.Infrastructure.Workers;

/// <summary>Background worker for commerce marketplace outbox.</summary>
public sealed class MarketplaceSyncWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<CommerceOptions> _options;
    private readonly ILogger<MarketplaceSyncWorker> _logger;

    public MarketplaceSyncWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<CommerceOptions> options,
        ILogger<MarketplaceSyncWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(1, _options.Value.PollIntervalSeconds));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<MarketplaceSyncProcessor>();
                var processed = await processor.ProcessDueAsync(
                    _options.Value.SyncBatchSize,
                    DateTimeOffset.UtcNow,
                    stoppingToken);
                if (processed > 0)
                {
                    _logger.LogDebug("Commerce marketplace sync processed {Count} jobs", processed);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Commerce marketplace sync worker failed");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }
}
