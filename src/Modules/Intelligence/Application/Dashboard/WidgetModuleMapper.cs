using Core.Domain.Entitlements;
using Intelligence.Domain.Dashboard;

namespace Intelligence.Application.Dashboard;

/// <summary>Maps dashboard widgets to billable modules for entitlement checks.</summary>
public static class WidgetModuleMapper
{
    /// <summary>Returns the commercial module required to load the widget data, if any.</summary>
    public static CommercialModule? GetRequiredModule(string widgetKey) => widgetKey switch
    {
        WidgetCatalog.SalesToday or WidgetCatalog.SalesByHour => CommercialModule.Sales,
        WidgetCatalog.GroomingToday => CommercialModule.Petshop,
        WidgetCatalog.ClinicalAppointmentsToday => CommercialModule.Veterinary,
        WidgetCatalog.OnlineOrdersToday => CommercialModule.Commerce,
        _ => null
    };
}
