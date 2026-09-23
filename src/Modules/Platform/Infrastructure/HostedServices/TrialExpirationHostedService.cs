using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Platform.Application.Subscriptions;

namespace Platform.Infrastructure.HostedServices;

/// <summary>Periodically expires due tenant trials (9.3).</summary>
public sealed class TrialExpirationHostedService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TrialExpirationHostedService> _logger;

    /// <summary>Creates the background service.</summary>
    public TrialExpirationHostedService(IServiceScopeFactory scopeFactory, ILogger<TrialExpirationHostedService> logger)
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
                await mediator.Send(new ExpireDueTrialsCommand(DateTimeOffset.UtcNow), stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Trial expiration sweep failed.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}
