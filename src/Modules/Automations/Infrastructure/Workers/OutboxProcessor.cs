using Automations.Application.Jobs;
using Automations.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Automations.Infrastructure.Workers;

/// <summary>
/// Background worker that polls the SQL outbox and processes due message jobs.
/// </summary>
public sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<AutomationsOptions> _options;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(
        IServiceScopeFactory scopeFactory,
        IOptions<AutomationsOptions> options,
        ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pollInterval = TimeSpan.FromSeconds(Math.Max(1, _options.Value.PollIntervalSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<MessageJobProcessor>();
                var processed = await processor.ProcessDueAsync(
                    _options.Value.BatchSize,
                    DateTimeOffset.UtcNow,
                    stoppingToken);

                if (processed > 0)
                {
                    _logger.LogDebug("Automations outbox processed {Count} jobs", processed);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Automations outbox worker cycle failed");
            }

            await Task.Delay(pollInterval, stoppingToken);
        }
    }
}
