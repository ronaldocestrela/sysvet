using Platform.Domain.Services;

namespace Platform.Application.Abstractions;

/// <summary>Loads platform billing data for SaaS KPI aggregation (10.2).</summary>
public interface ISaasMetricsReader
{
    /// <summary>Builds the metrics snapshot for a civil month.</summary>
    Task<SaasMetricsSnapshot> GetSnapshotAsync(int year, int month, CancellationToken cancellationToken = default);
}
