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
            ["commission-rules"] = new(AppRoutes.SalesCommissionRules, "Comissões", "bi-percent"),
            ["cash"] = new(AppRoutes.SalesCashRegister, "Caixa", "bi-wallet2"),
            ["prepaid-balances"] = new(AppRoutes.SalesPrepaidBalances, "Pacotes pré-pagos", "bi-ticket-perforated"),
            ["tutors"] = new(AppRoutes.Tutors, "Tutores", "bi-people"),
            ["pets"] = new(AppRoutes.Pets, "Pets", "bi-heart"),
            ["appointments"] = new(AppRoutes.Appointments, "Agenda", "bi-calendar-event"),
            ["vaccines"] = new(AppRoutes.VaccineAlerts, "Alertas de vacina", "bi-exclamation-triangle"),
            ["hospitalizations"] = new(AppRoutes.Hospitalizations, "Internação", "bi-hospital"),
            ["quotes"] = new(AppRoutes.PendingQuoteConversions, "Orçamentos pendentes", "bi-receipt"),
            ["inventory"] = new(AppRoutes.Products, "Produtos", "bi-box-seam"),
            ["suppliers"] = new(AppRoutes.Suppliers, "Fornecedores", "bi-truck"),
            ["purchase-imports"] = new(AppRoutes.PurchaseImports, "Entrada NF-e", "bi-file-earmark-code"),
            ["stock"] = new(AppRoutes.StockMovements, "Movimentações", "bi-arrow-left-right"),
            ["stock-alerts"] = new(AppRoutes.StockAlerts, "Alertas de estoque", "bi-exclamation-diamond"),
            ["inventory-counts"] = new(AppRoutes.InventoryCounts, "Inventário", "bi-upc-scan"),
            ["purchase-suggestions"] = new(AppRoutes.PurchaseSuggestions, "Sugestão de compras", "bi-cart-plus"),
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
