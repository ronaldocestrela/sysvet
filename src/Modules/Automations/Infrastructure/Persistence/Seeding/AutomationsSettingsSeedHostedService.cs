using Automations.Domain.Entities;
using Automations.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Automations.Infrastructure.Persistence.Seeding;

/// <summary>
/// Ensures default Automations settings exist after migrations are applied.
/// </summary>
public sealed class AutomationsSettingsSeedHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AutomationsSettingsSeedHostedService> _logger;

    public AutomationsSettingsSeedHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<AutomationsSettingsSeedHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AutomationsDbContext>();
            if (!await db.Database.CanConnectAsync(cancellationToken))
            {
                return;
            }

            if ((await db.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
            {
                return;
            }

            var repo = scope.ServiceProvider.GetRequiredService<IAutomationsSettingsRepository>();
            if (await repo.GetSingletonAsync(cancellationToken) is not null)
            {
                return;
            }

            var created = AutomationsSettings.CreateDefault();
            if (created.IsSuccess)
            {
                repo.Add(created.Value);
                await scope.ServiceProvider.GetRequiredService<IAutomationsUnitOfWork>().SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Automations default settings seeded.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Automations settings seed failed.");
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
