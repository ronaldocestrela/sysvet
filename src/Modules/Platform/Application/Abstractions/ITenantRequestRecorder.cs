namespace Platform.Application.Abstractions;

/// <summary>Increments daily tenant request counters (9.7).</summary>
public interface ITenantRequestRecorder
{
    /// <summary>Records one API request for the tenant when id is set.</summary>
    Task RecordAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
