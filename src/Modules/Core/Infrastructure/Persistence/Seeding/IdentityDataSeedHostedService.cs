using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Core.Infrastructure.Persistence.Seeding;

/// <summary>
/// Runs identity reference data seeding once at application startup after the host is built.
/// </summary>
public sealed class IdentityDataSeedHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<IdentityDataSeedHostedService> _logger;

    public IdentityDataSeedHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<IdentityDataSeedHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<IIdentityDataSeeder>();
            await seeder.SeedAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Identity data seeding failed.");
            throw;
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
