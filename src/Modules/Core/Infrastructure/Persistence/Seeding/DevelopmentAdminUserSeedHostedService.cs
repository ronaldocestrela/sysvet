using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Core.Infrastructure.Persistence.Seeding;

/// <summary>
/// Runs the development admin user seeder when the host environment is Development.
/// </summary>
public sealed class DevelopmentAdminUserSeedHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<DevelopmentAdminUserSeedHostedService> _logger;

    /// <summary>Creates the hosted service.</summary>
    public DevelopmentAdminUserSeedHostedService(
        IServiceScopeFactory scopeFactory,
        IHostEnvironment environment,
        ILogger<DevelopmentAdminUserSeedHostedService> logger)
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
            using var scope = _scopeFactory.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<IDevelopmentAdminUserSeeder>();
            await seeder.SeedAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Development admin user seeding failed.");
            throw;
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
