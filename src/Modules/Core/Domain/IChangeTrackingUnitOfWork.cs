namespace Core.Domain;

/// <summary>
/// Unit of work that exposes whether EF (or another store) has pending changes before commit.
/// </summary>
public interface IChangeTrackingUnitOfWork : IUnitOfWork
{
    /// <summary>
    /// Returns true when there are tracked entities pending persistence.
    /// </summary>
    bool HasPendingChanges();
}
