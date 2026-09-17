namespace Clients.Infrastructure.Persistence;

/// <summary>
/// Persists the SQLite database file across browser sessions (WASM) or no-ops on native hosts.
/// </summary>
public interface ISqliteFilePersistence
{
    /// <summary>
    /// Restores a previously saved database file into the virtual filesystem, when present.
    /// </summary>
    Task RestoreIfExistsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the current database file bytes to durable storage.
    /// </summary>
    Task PersistAsync(byte[] databaseBytes, CancellationToken cancellationToken = default);
}
