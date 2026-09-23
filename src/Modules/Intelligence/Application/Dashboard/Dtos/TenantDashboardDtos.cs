namespace Intelligence.Application.Dashboard.Dtos;

/// <summary>Operational dashboard payload for the current user.</summary>
public sealed class TenantDashboardDto
{
    /// <summary>Business calendar date in America/Sao_Paulo.</summary>
    public DateOnly BusinessDate { get; init; }

    /// <summary>Widgets in display order.</summary>
    public IReadOnlyList<DashboardWidgetDto> Widgets { get; init; } = Array.Empty<DashboardWidgetDto>();
}

/// <summary>Single widget with optional KPI payload or per-widget error.</summary>
public sealed class DashboardWidgetDto
{
    /// <summary>Widget key from the catalog.</summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>When false, <see cref="ErrorCode"/> explains the failure.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Stable error code when <see cref="IsSuccess"/> is false.</summary>
    public string? ErrorCode { get; init; }

    /// <summary>Typed KPI object serialized to JSON (sales, grooming, etc.).</summary>
    public object? Data { get; init; }
}

/// <summary>Layout definition exposed to administrators.</summary>
public sealed class ProfileDashboardLayoutDto
{
    /// <summary>Access profile identifier.</summary>
    public Guid AccessProfileId { get; init; }

    /// <summary>Ordered widget slots.</summary>
    public IReadOnlyList<DashboardWidgetSlotDto> Slots { get; init; } = Array.Empty<DashboardWidgetSlotDto>();
}

/// <summary>Widget slot for layout APIs.</summary>
public sealed class DashboardWidgetSlotDto
{
    /// <summary>Widget key.</summary>
    public string WidgetKey { get; init; } = string.Empty;

    /// <summary>Visibility flag.</summary>
    public bool IsVisible { get; init; }

    /// <summary>Sort order.</summary>
    public int SortOrder { get; init; }
}
