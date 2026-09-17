namespace Core.Application.Sync;

/// <summary>
/// Reads CRM changes for sync pull, including tombstones and tenant isolation.
/// </summary>
public interface ISyncChangeFeedReader
{
    /// <summary>
    /// Returns tutors and pets changed after <paramref name="since"/> for the current tenant.
    /// </summary>
    Task<PullChangesResult> ReadChangesAsync(DateTimeOffset since, int take, CancellationToken cancellationToken);
}
