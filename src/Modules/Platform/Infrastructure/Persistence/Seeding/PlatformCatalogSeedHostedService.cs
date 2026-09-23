using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Platform.Domain.Catalog;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Persistence.Seeding;

/// <summary>Seeds default plans and add-ons (roadmap 9.3).</summary>
public sealed class PlatformCatalogSeedHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PlatformCatalogSeedHostedService> _logger;

    /// <summary>Creates the hosted seeder.</summary>
    public PlatformCatalogSeedHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<PlatformCatalogSeedHostedService> logger)
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
            var context = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
            await context.Database.MigrateAsync(cancellationToken);

            var planRepository = scope.ServiceProvider.GetRequiredService<IPlanRepository>();
            if (await planRepository.GetByCodeAsync(CatalogCodes.Plans.Starter, cancellationToken) is null)
            {
                foreach (var plan in CatalogSeedData.CreateDefaultPlans())
                {
                    await planRepository.AddAsync(plan, cancellationToken);
                }
            }

            var addOnRepository = scope.ServiceProvider.GetRequiredService<IAddOnRepository>();
            if (await addOnRepository.GetByCodeAsync(CatalogCodes.AddOns.Fiscal, cancellationToken) is null)
            {
                foreach (var addOn in CatalogSeedData.CreateDefaultAddOns())
                {
                    await addOnRepository.AddAsync(addOn, cancellationToken);
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Platform catalog seed skipped.");
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
