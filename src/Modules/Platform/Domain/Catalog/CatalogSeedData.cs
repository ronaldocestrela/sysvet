using Core.Domain.Entitlements;
using Platform.Domain.Entities;

namespace Platform.Domain.Catalog;

/// <summary>Default catalog composition and prices for idempotent seed (9.3).</summary>
public static class CatalogSeedData
{
    /// <summary>Well-known plan ids for seed idempotency.</summary>
    public static readonly Guid StarterPlanId = Guid.Parse("11111111-1111-4111-8111-111111111101");

    /// <summary>Pro plan id.</summary>
    public static readonly Guid ProPlanId = Guid.Parse("11111111-1111-4111-8111-111111111102");

    /// <summary>Hospital plan id.</summary>
    public static readonly Guid HospitalPlanId = Guid.Parse("11111111-1111-4111-8111-111111111103");

    /// <summary>Estetica add-on id.</summary>
    public static readonly Guid EsteticaAddOnId = Guid.Parse("22222222-2222-4222-8222-222222222201");

    /// <summary>Fiscal add-on id.</summary>
    public static readonly Guid FiscalAddOnId = Guid.Parse("22222222-2222-4222-8222-222222222202");

    /// <summary>Automacao add-on id.</summary>
    public static readonly Guid AutomacaoAddOnId = Guid.Parse("22222222-2222-4222-8222-222222222203");

    /// <summary>PdvOffline add-on id.</summary>
    public static readonly Guid PdvOfflineAddOnId = Guid.Parse("22222222-2222-4222-8222-222222222204");

    /// <summary>Modules included in Starter.</summary>
    public static IReadOnlyList<CommercialModule> StarterModules { get; } = [CommercialModule.Veterinary];

    /// <summary>Modules included in Pro (includes Starter).</summary>
    public static IReadOnlyList<CommercialModule> ProModules { get; } =
    [
        CommercialModule.Veterinary,
        CommercialModule.Inventory,
        CommercialModule.Finance,
        CommercialModule.ClinicSite,
        CommercialModule.Commerce,
        CommercialModule.TutorPortal
    ];

    /// <summary>Modules included in Hospital24h (includes Pro + Hospital).</summary>
    public static IReadOnlyList<CommercialModule> HospitalModules { get; } =
        ProModules.Append(CommercialModule.Hospital).ToList();

    /// <summary>Builds default plans for seed.</summary>
    public static IReadOnlyList<Plan> CreateDefaultPlans()
    {
        var starter = Plan.Create(StarterPlanId, CatalogCodes.Plans.Starter, "Starter", 199m, StarterModules).Value;
        var pro = Plan.Create(ProPlanId, CatalogCodes.Plans.Pro, "Pro", 399m, ProModules).Value;
        var hospital = Plan.Create(HospitalPlanId, CatalogCodes.Plans.Hospital24h, "Hospital 24h", 699m, HospitalModules).Value;
        return [starter, pro, hospital];
    }

    /// <summary>Builds default add-ons for seed.</summary>
    public static IReadOnlyList<AddOn> CreateDefaultAddOns()
    {
        return
        [
            AddOn.Create(EsteticaAddOnId, CatalogCodes.AddOns.Estetica, "Estética", 49m, CommercialModule.Petshop).Value,
            AddOn.Create(FiscalAddOnId, CatalogCodes.AddOns.Fiscal, "Fiscal", 99m, CommercialModule.Fiscal).Value,
            AddOn.Create(AutomacaoAddOnId, CatalogCodes.AddOns.Automacao, "Automação", 79m, CommercialModule.Automations).Value,
            AddOn.Create(PdvOfflineAddOnId, CatalogCodes.AddOns.PdvOffline, "PDV Offline", 89m, CommercialModule.Sales).Value
        ];
    }
}
