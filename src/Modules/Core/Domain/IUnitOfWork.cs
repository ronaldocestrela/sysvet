namespace Core.Domain;

/// <summary>
/// Unit of work boundary that commits pending repository changes to the underlying store.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all pending changes tracked by this context.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
