namespace SharedUI.Navigation;

/// <summary>
/// Maps API menu keys from <c>/auth/me</c> to sidebar entries.
/// </summary>
public static class MenuNavigation
{
    private static readonly IReadOnlyDictionary<string, AppNavItem> MenuKeyMap =
        new Dictionary<string, AppNavItem>(StringComparer.OrdinalIgnoreCase)
        {
            ["sales"] = new(AppRoutes.SalesPos, "PDV", "bi-cart"),
            ["cash"] = new(AppRoutes.SalesCashRegister, "Caixa", "bi-wallet2"),
            ["tutors"] = new(AppRoutes.Tutors, "Tutores", "bi-people"),
            ["pets"] = new(AppRoutes.Pets, "Pets", "bi-heart"),
            ["appointments"] = new(AppRoutes.Appointments, "Agenda", "bi-calendar-event"),
            ["inventory"] = new(AppRoutes.Products, "Produtos", "bi-box-seam"),
            ["audit"] = new(AppRoutes.Home, "Auditoria", "bi-journal-text")
        };

    /// <summary>
    /// Builds nav items for the granted menu keys; dashboard is always included first.
    /// </summary>
    public static IReadOnlyList<AppNavItem> BuildNavItems(IReadOnlyList<string> menuKeys)
    {
        var items = new List<AppNavItem>
        {
            new(AppRoutes.Home, "Dashboard", "bi-house-door-fill")
        };

        foreach (var key in menuKeys)
        {
            if (MenuKeyMap.TryGetValue(key, out var item) && items.All(i => i.Href != item.Href))
            {
                items.Add(item);
            }
        }

        if (menuKeys.Count == 0)
        {
            return AppNavItems.All;
        }

        return items;
    }
}
