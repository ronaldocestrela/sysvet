namespace Intelligence.Domain.Dashboard;

/// <summary>Known dashboard widget keys for tenant operational BI (roadmap 10.1).</summary>
public static class WidgetCatalog
{
    public const string SalesToday = "SalesToday";
    public const string SalesByHour = "SalesByHour";
    public const string GroomingToday = "GroomingToday";
    public const string ClinicalAppointmentsToday = "ClinicalAppointmentsToday";
    public const string OnlineOrdersToday = "OnlineOrdersToday";

    /// <summary>All widget keys in default display order.</summary>
    public static IReadOnlyList<string> All { get; } =
    [
        SalesToday,
        SalesByHour,
        GroomingToday,
        ClinicalAppointmentsToday,
        OnlineOrdersToday
    ];

    /// <summary>Returns whether the key is part of the catalog.</summary>
    public static bool IsValid(string widgetKey) =>
        All.Contains(widgetKey, StringComparer.Ordinal);
}
