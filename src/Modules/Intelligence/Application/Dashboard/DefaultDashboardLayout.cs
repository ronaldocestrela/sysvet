using Core.Application.Authorization;
using Intelligence.Domain.Dashboard;

namespace Intelligence.Application.Dashboard;

/// <summary>System default widget layouts per Identity base role when no custom layout is stored.</summary>
public static class DefaultDashboardLayout
{
    /// <summary>Returns default slots for a system profile base role.</summary>
    public static IReadOnlyList<DashboardWidgetSlot> ForBaseRole(string baseRole)
    {
        var keys = baseRole switch
        {
            ApplicationRoles.Admin => WidgetCatalog.All,
            ApplicationRoles.Cashier => new[] { WidgetCatalog.SalesToday, WidgetCatalog.SalesByHour },
            ApplicationRoles.Receptionist => new[] { WidgetCatalog.GroomingToday, WidgetCatalog.ClinicalAppointmentsToday },
            ApplicationRoles.Veterinarian => new[] { WidgetCatalog.ClinicalAppointmentsToday },
            _ => Array.Empty<string>()
        };

        return keys.Select((key, index) => new DashboardWidgetSlot
        {
            WidgetKey = key,
            IsVisible = true,
            SortOrder = index
        }).ToList();
    }
}
