namespace Core.Application.IntegrationEvents;

/// <summary>Paid POS sales totals for the current business day.</summary>
public sealed class SalesTodayKpiSnapshot
{
    /// <summary>Net revenue after same-day returns.</summary>
    public decimal NetAmount { get; init; }

    /// <summary>Count of paid orders in the period.</summary>
    public int OrderCount { get; init; }

    /// <summary>Average ticket (zero when no orders).</summary>
    public decimal AverageTicket { get; init; }
}

/// <summary>Single hour bucket for intraday sales chart.</summary>
public sealed class SalesHourBucketSnapshot
{
    /// <summary>Local hour 0–23 in the clinic business timezone.</summary>
    public int Hour { get; init; }

    /// <summary>Net amount for the hour.</summary>
    public decimal Amount { get; init; }

    /// <summary>Paid order count in the hour.</summary>
    public int OrderCount { get; init; }
}

/// <summary>Hourly paid sales for the business day.</summary>
public sealed class SalesByHourKpiSnapshot
{
    /// <summary>Twenty-four buckets (missing hours are zero-filled by the consumer).</summary>
    public IReadOnlyList<SalesHourBucketSnapshot> Buckets { get; init; } = Array.Empty<SalesHourBucketSnapshot>();
}

/// <summary>Grooming appointment counts grouped by status for the business day.</summary>
public sealed class GroomingTodayKpiSnapshot
{
    /// <summary>Status name to count.</summary>
    public IReadOnlyDictionary<string, int> ByStatus { get; init; } = new Dictionary<string, int>();
}

/// <summary>Clinical appointment counts grouped by status for the business day.</summary>
public sealed class ClinicalAppointmentsTodayKpiSnapshot
{
    /// <summary>Status name to count.</summary>
    public IReadOnlyDictionary<string, int> ByStatus { get; init; } = new Dictionary<string, int>();
}

/// <summary>Confirmed online commerce orders for the business day.</summary>
public sealed class OnlineOrdersTodayKpiSnapshot
{
    /// <summary>Order count with confirmation in the period.</summary>
    public int OrderCount { get; init; }

    /// <summary>Sum of line totals at confirmation.</summary>
    public decimal TotalAmount { get; init; }
}
