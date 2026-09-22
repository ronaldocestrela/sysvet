using Automations.Infrastructure.Configuration;
using Automations.Infrastructure.Reminders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Automations.Infrastructure.Workers;

/// <summary>
/// Periodically scans reminder candidates and enqueues outbound jobs for the current tenant scope.
/// </summary>
public sealed class ReminderScheduler : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReminderScheduler> _logger;
    private readonly TimeSpan _interval;

    public ReminderScheduler(
        IServiceScopeFactory scopeFactory,
        IOptions<AutomationsOptions> options,
        ILogger<ReminderScheduler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        var minutes = Math.Max(1, options.Value.ReminderScanIntervalMinutes);
        _interval = TimeSpan.FromMinutes(minutes);
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var scan = scope.ServiceProvider.GetRequiredService<ReminderScanService>();
                var enqueued = await scan.ScanAsync(stoppingToken);
                if (enqueued > 0)
                {
                    _logger.LogInformation("Reminder scan enqueued {Count} job(s).", enqueued);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reminder scheduler cycle failed.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
