using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Platform.Infrastructure.Persistence.Seeding;

/// <summary>Runs Super Admin seed on startup in Development.</summary>
public sealed class DevelopmentSuperAdminSeedHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<DevelopmentSuperAdminSeedHostedService> _logger;

    /// <summary>Creates the hosted service.</summary>
    public DevelopmentSuperAdminSeedHostedService(
        IServiceScopeFactory scopeFactory,
        IHostEnvironment environment,
        ILogger<DevelopmentSuperAdminSeedHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _environment = environment;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            return;
        }

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var seeder = scope.ServiceProvider.GetRequiredService<IDevelopmentSuperAdminSeeder>();
            await seeder.SeedAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Super Admin seed skipped.");
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
