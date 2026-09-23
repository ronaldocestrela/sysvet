namespace Intelligence.Domain.Dashboard;

/// <summary>Ordered visibility entry for a dashboard widget on an access profile.</summary>
public sealed class DashboardWidgetSlot
{
    /// <summary>Widget key from <see cref="WidgetCatalog"/>.</summary>
    public string WidgetKey { get; init; } = string.Empty;

    /// <summary>When false the widget is hidden for users with this profile.</summary>
    public bool IsVisible { get; init; }

    /// <summary>Zero-based display order.</summary>
    public int SortOrder { get; init; }
}
