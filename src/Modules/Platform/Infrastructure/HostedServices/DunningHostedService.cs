using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Platform.Application.Dunning;

namespace Platform.Infrastructure.HostedServices;

/// <summary>Periodically runs SaaS dunning sweep (9.5).</summary>
public sealed class DunningHostedService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DunningHostedService> _logger;

    /// <summary>Creates the background service.</summary>
    public DunningHostedService(IServiceScopeFactory scopeFactory, ILogger<DunningHostedService> logger)
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
                await mediator.Send(new ProcessDunningCommand(DateTimeOffset.UtcNow), stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Dunning sweep failed.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}
