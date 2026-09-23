namespace Platform.Application.Health;

/// <summary>Documented constants for tenant health estimates (9.7 / ADR-052).</summary>
public static class TenantHealthMetrics
{
    /// <summary>Average bytes per operational row for storage estimates.</summary>
    public const int BytesPerRowEstimate = 512;
}
