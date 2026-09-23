using Platform.Domain.Entities;

namespace Platform.Domain.Repositories;

/// <summary>Append-only impersonation audit persistence (9.6).</summary>
public interface IImpersonationAuditRepository
{
    /// <summary>Appends an audit entry (insert only).</summary>
    Task AddAsync(ImpersonationAuditEntry entry, CancellationToken cancellationToken = default);

    /// <summary>Lists audit entries newest first.</summary>
    Task<IReadOnlyList<ImpersonationAuditEntry>> ListAsync(int take, CancellationToken cancellationToken = default);

    /// <summary>Paged impersonation audits (newest first).</summary>
    Task<(IReadOnlyList<ImpersonationAuditEntry> Items, int TotalCount)> ListPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
