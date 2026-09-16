using Core.Domain.Auditing;

namespace Core.Domain;

/// <summary>
/// Read-only persistence port for tenant-scoped audit log queries.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Searches audit entries with server-side paging and optional filters.
    /// </summary>
    Task<PagedList<AuditLog>> SearchAsync(
        int page,
        int pageSize,
        string? entityName,
        Guid? entityId,
        string? action,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default);
}
