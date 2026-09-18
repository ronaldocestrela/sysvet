namespace SharedUI.Navigation;

/// <summary>
/// Static sidebar entries for phase 3.1; permission filtering arrives in 3.2 via API menus.
/// </summary>
public static class AppNavItems
{
    /// <summary>Ordered navigation entries shown in <see cref="Layout.NavMenu"/>.</summary>
    public static IReadOnlyList<AppNavItem> All { get; } =
    [
        new(AppRoutes.SalesPos, "PDV", "bi-cart"),
        new(AppRoutes.SalesCashRegister, "Caixa", "bi-wallet2"),
        new(AppRoutes.Home, "Dashboard", "bi-house-door-fill"),
        new(AppRoutes.Tutors, "Tutores", "bi-people"),
        new(AppRoutes.Pets, "Pets", "bi-heart"),
        new(AppRoutes.Appointments, "Agenda", "bi-calendar-event"),
        new(AppRoutes.VaccineAlerts, "Alertas de vacina", "bi-exclamation-triangle"),
        new(AppRoutes.Hospitalizations, "Internação", "bi-hospital"),
        new(AppRoutes.Products, "Produtos", "bi-box-seam"),
        new(AppRoutes.StockMovements, "Estoque", "bi-arrow-left-right"),
    ];
}

/// <summary>
/// A single sidebar navigation link.
/// </summary>
/// <param name="Href">Route path.</param>
/// <param name="Label">Display label (Portuguese UI).</param>
/// <param name="IconCss">Bootstrap Icons class suffix (without <c>bi</c> prefix).</param>
public readonly record struct AppNavItem(string Href, string Label, string IconCss);
