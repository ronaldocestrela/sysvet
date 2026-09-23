using Platform.Domain.Entities;

namespace Platform.Domain.Repositories;

/// <summary>Append-only Super Admin change audit persistence (9.7).</summary>
public interface IPlatformChangeAuditRepository
{
    /// <summary>Persists a change audit row.</summary>
    Task AddAsync(PlatformChangeAuditEntry entry, CancellationToken cancellationToken = default);

    /// <summary>Lists recent change audits optionally filtered by tenant.</summary>
    Task<IReadOnlyList<PlatformChangeAuditEntry>> ListAsync(Guid? tenantId, int take, CancellationToken cancellationToken = default);
}
