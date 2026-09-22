using Automations.Application.Jobs;
using Automations.Infrastructure.Configuration;
using Core.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Platform.Application.Tenancy;

namespace Automations.Infrastructure.Workers;

/// <summary>
/// Background worker that polls the SQL outbox and processes due message jobs per tenant catalog entry.
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
                IReadOnlyList<TenantDirectoryEntry> tenants = [];
                try
                {
                    var directory = scope.ServiceProvider.GetRequiredService<ITenantDirectory>();
                    tenants = await directory.ListAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Tenant directory unavailable; using single scope.");
                }

                if (tenants.Count == 0)
                {
                    await ProcessSingleScopeAsync(scope.ServiceProvider, stoppingToken);
                }
                else
                {
                    foreach (var tenant in tenants)
                    {
                        await using var tenantScope = _scopeFactory.CreateAsyncScope();
                        var tenantContext = tenantScope.ServiceProvider.GetRequiredService<ITenantContext>();
                        tenantContext.TenantId = tenant.TenantId;
                        tenantContext.SchemaName = tenant.SchemaName;
                        await ProcessSingleScopeAsync(tenantScope.ServiceProvider, stoppingToken);
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Automations outbox worker cycle failed");
            }

            await Task.Delay(pollInterval, stoppingToken);
        }
    }

    private async Task ProcessSingleScopeAsync(IServiceProvider provider, CancellationToken stoppingToken)
    {
        var processor = provider.GetRequiredService<MessageJobProcessor>();
        var processed = await processor.ProcessDueAsync(
            _options.Value.BatchSize,
            DateTimeOffset.UtcNow,
            stoppingToken);

        if (processed > 0)
        {
            _logger.LogDebug("Automations outbox processed {Count} jobs", processed);
        }
    }
}
