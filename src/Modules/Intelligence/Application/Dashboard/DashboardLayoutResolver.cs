using Intelligence.Domain.Dashboard;
using Intelligence.Domain.Entities;

namespace Intelligence.Application.Dashboard;

/// <summary>Merges persisted layouts with role defaults.</summary>
public static class DashboardLayoutResolver
{
    /// <summary>Returns effective visible slots ordered for rendering.</summary>
    public static IReadOnlyList<DashboardWidgetSlot> ResolveVisibleSlots(
        ProfileDashboardLayout? persistedLayout,
        string baseRole)
    {
        var slots = persistedLayout?.Slots.ToList()
                    ?? DefaultDashboardLayout.ForBaseRole(baseRole).ToList();

        return slots
            .Where(s => s.IsVisible)
            .OrderBy(s => s.SortOrder)
            .ToList();
    }
}
