namespace SharedUI.Navigation;

/// <summary>
/// Central route paths for client navigation to avoid magic strings across layouts and pages.
/// </summary>
public static class AppRoutes
{
    /// <summary>Application home / dashboard.</summary>
    public const string Home = "/";

    /// <summary>Staff login.</summary>
    public const string Login = "/login";

    /// <summary>CRM tutors list.</summary>
    public const string Tutors = "/tutors";

    /// <summary>CRM pets list.</summary>
    public const string Pets = "/pets";

    /// <summary>Clinical appointments.</summary>
    public const string Appointments = "/appointments";

    /// <summary>Hospitalizations.</summary>
    public const string Hospitalizations = "/hospitalizations";

    /// <summary>Inventory products.</summary>
    public const string Products = "/products";

    /// <summary>Stock movements.</summary>
    public const string StockMovements = "/stock-movements";

    /// <summary>Point of sale terminal.</summary>
    public const string SalesPos = "/sales/pos";

    /// <summary>Cash register.</summary>
    public const string SalesCashRegister = "/sales/cash-register";
}
