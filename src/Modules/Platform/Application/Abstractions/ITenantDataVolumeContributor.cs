namespace Platform.Application.Abstractions;

/// <summary>Contributes row counts toward tenant data volume estimates (9.7).</summary>
public interface ITenantDataVolumeContributor
{
    /// <summary>Module label for the slice.</summary>
    string ModuleName { get; }

    /// <summary>Counts rows owned by the tenant in this module.</summary>
    Task<long> CountRowsAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
