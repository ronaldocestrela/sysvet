using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Platform.Application.Billing;

namespace Platform.Infrastructure.HostedServices;

/// <summary>Periodically charges due tenant subscriptions (9.4).</summary>
public sealed class BillingCycleHostedService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BillingCycleHostedService> _logger;

    /// <summary>Creates the background service.</summary>
    public BillingCycleHostedService(IServiceScopeFactory scopeFactory, ILogger<BillingCycleHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(new ChargeDueSubscriptionsCommand(DateTimeOffset.UtcNow), stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Billing cycle sweep failed.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}
