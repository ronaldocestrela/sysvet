namespace Core.Application.Caching;

/// <summary>Standard cache TTLs for Fase 10.4 (ADR-057).</summary>
public static class CacheDurations
{
    /// <summary>Dashboard poll interval alignment (30s).</summary>
    public static readonly TimeSpan Dashboard = TimeSpan.FromSeconds(30);

    /// <summary>BI reports, SaaS metrics, adoption heatmap, entitlements.</summary>
    public static readonly TimeSpan Analytics = TimeSpan.FromMinutes(5);
}
