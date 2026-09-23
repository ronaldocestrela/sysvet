using Microsoft.Extensions.DependencyInjection;
using Platform.Domain.Catalog;
using Platform.Domain.Repositories;
using Platform.Infrastructure.Persistence;

namespace API.IntegrationTests.Platform;

/// <summary>Seeds Platform commercial catalog for integration tests (9.3).</summary>
internal static class PlatformCatalogTestSeeder
{
    /// <summary>Idempotently inserts default plans and add-ons.</summary>
    public static async Task EnsureCatalogAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var planRepository = services.GetRequiredService<IPlanRepository>();
        if (await planRepository.GetByCodeAsync(CatalogCodes.Plans.Starter, cancellationToken) is null)
        {
            foreach (var plan in CatalogSeedData.CreateDefaultPlans())
            {
                await planRepository.AddAsync(plan, cancellationToken);
            }
        }

        var addOnRepository = services.GetRequiredService<IAddOnRepository>();
        if (await addOnRepository.GetByCodeAsync(CatalogCodes.AddOns.Fiscal, cancellationToken) is null)
        {
            foreach (var addOn in CatalogSeedData.CreateDefaultAddOns())
            {
                await addOnRepository.AddAsync(addOn, cancellationToken);
            }
        }

        await services.GetRequiredService<PlatformDbContext>().SaveChangesAsync(cancellationToken);
    }
}
