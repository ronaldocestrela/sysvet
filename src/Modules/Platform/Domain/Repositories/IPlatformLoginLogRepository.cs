using Platform.Domain.Entities;

namespace Platform.Domain.Repositories;

/// <summary>Append-only platform login log persistence (9.7).</summary>
public interface IPlatformLoginLogRepository
{
    /// <summary>Persists a login log row.</summary>
    Task AddAsync(PlatformLoginLog log, CancellationToken cancellationToken = default);

    /// <summary>Lists recent login logs optionally filtered by tenant.</summary>
    Task<IReadOnlyList<PlatformLoginLog>> ListAsync(Guid? tenantId, int take, CancellationToken cancellationToken = default);

    /// <summary>Paged login logs (newest first).</summary>
    Task<(IReadOnlyList<PlatformLoginLog> Items, int TotalCount)> ListPagedAsync(
        Guid? tenantId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
