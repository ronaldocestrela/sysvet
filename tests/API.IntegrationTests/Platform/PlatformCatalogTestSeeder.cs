using Core.Domain.Entitlements;
using Microsoft.Extensions.DependencyInjection;
using Platform.Domain.Catalog;
using Platform.Domain.Entities;
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

        if (await addOnRepository.GetByCodeAsync(CatalogCodes.AddOns.Fiscal, cancellationToken) is not null
            && await addOnRepository.GetByCodeAsync(CatalogCodes.AddOns.Intelligence, cancellationToken) is null)
        {
            var intelligence = CatalogSeedData.CreateDefaultAddOns()
                .First(a => a.Code == CatalogCodes.AddOns.Intelligence);
            await addOnRepository.AddAsync(intelligence, cancellationToken);
        }

        var context = services.GetRequiredService<PlatformDbContext>();
        await EnsureIntelligenceOnPaidPlansAsync(planRepository, context, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureIntelligenceOnPaidPlansAsync(
        IPlanRepository planRepository,
        PlatformDbContext context,
        CancellationToken cancellationToken)
    {
        foreach (var code in new[] { CatalogCodes.Plans.Pro, CatalogCodes.Plans.Hospital24h })
        {
            var plan = await planRepository.GetByCodeAsync(code, cancellationToken);
            if (plan is null || plan.GetModuleList().Contains(CommercialModule.Intelligence))
            {
                continue;
            }

            plan.IncludedModules.Add(new PlanIncludedModule(plan.Id, CommercialModule.Intelligence));
            context.Update(plan);
        }
    }
}
