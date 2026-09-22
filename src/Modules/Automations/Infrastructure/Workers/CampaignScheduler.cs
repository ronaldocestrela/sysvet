using Automations.Infrastructure.Campaigns;
using Automations.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Automations.Infrastructure.Workers;

/// <summary>
/// Periodically scans for post-appointment NPS opportunities.
/// </summary>
public sealed class CampaignScheduler : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CampaignScheduler> _logger;
    private readonly AutomationsOptions _options;

    public CampaignScheduler(
        IServiceScopeFactory scopeFactory,
        ILogger<CampaignScheduler> logger,
        IOptions<AutomationsOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMinutes(Math.Max(1, _options.CampaignScanIntervalMinutes));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var scan = scope.ServiceProvider.GetRequiredService<CampaignScanService>();
                var count = await scan.ScanAsync(stoppingToken);
                if (count > 0)
                {
                    _logger.LogInformation("Campaign scan enqueued {Count} NPS job(s).", count);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Campaign scheduler scan failed.");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }
}
