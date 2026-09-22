namespace Platform.Application.Tenancy;

/// <summary>
/// Read-only view of all tenants for background workers (ADR-038 / Platform 9.1).
/// </summary>
public interface ITenantDirectory
{
    /// <summary>Lists tenant ids and schema names from the catalog.</summary>
    Task<IReadOnlyList<TenantDirectoryEntry>> ListAsync(CancellationToken cancellationToken = default);
}

/// <summary>Minimal tenant metadata for worker scopes.</summary>
public sealed record TenantDirectoryEntry(Guid TenantId, string SchemaName, string Slug);
