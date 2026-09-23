using Core.Domain.Entitlements;

namespace Core.Application.Entitlements;

/// <summary>
/// Maps sidebar menu keys to commercial modules for entitlement filtering (9.3).
/// </summary>
public static class MenuEntitlementMapper
{
    private static readonly IReadOnlyDictionary<string, CommercialModule> MenuToModule =
        new Dictionary<string, CommercialModule>(StringComparer.OrdinalIgnoreCase)
        {
            ["tutors"] = CommercialModule.Veterinary,
            ["pets"] = CommercialModule.Veterinary,
            ["appointments"] = CommercialModule.Veterinary,
            ["vaccines"] = CommercialModule.Veterinary,
            ["quotes"] = CommercialModule.Veterinary,
            ["hospitalizations"] = CommercialModule.Hospital,
            ["grooming"] = CommercialModule.Petshop,
            ["inventory"] = CommercialModule.Inventory,
            ["suppliers"] = CommercialModule.Inventory,
            ["purchase-imports"] = CommercialModule.Inventory,
            ["stock"] = CommercialModule.Inventory,
            ["stock-alerts"] = CommercialModule.Inventory,
            ["inventory-counts"] = CommercialModule.Inventory,
            ["purchase-suggestions"] = CommercialModule.Inventory,
            ["sales"] = CommercialModule.Sales,
            ["commission-rules"] = CommercialModule.Sales,
            ["cash"] = CommercialModule.Sales,
            ["prepaid-balances"] = CommercialModule.Sales,
            ["finance"] = CommercialModule.Finance,
            ["fiscal"] = CommercialModule.Fiscal,
            ["automations"] = CommercialModule.Automations,
            ["clinic-site"] = CommercialModule.ClinicSite,
            ["commerce-offers"] = CommercialModule.Commerce,
            ["commerce-orders"] = CommercialModule.Commerce
        };

    /// <summary>Filters menu keys to those allowed by tenant entitlements.</summary>
    public static IReadOnlyList<string> FilterMenus(IReadOnlyList<string> menus, IReadOnlySet<CommercialModule> enabledModules)
    {
        var filtered = new List<string>(menus.Count);
        foreach (var key in menus)
        {
            if (!MenuToModule.TryGetValue(key, out var module) || enabledModules.Contains(module))
            {
                filtered.Add(key);
            }
        }

        return filtered;
    }
}
