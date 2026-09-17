namespace Clients.Infrastructure.Persistence;

/// <summary>
/// MAUI and tests use real filesystem paths; no extra persistence layer is required.
/// </summary>
public sealed class NoOpSqliteFilePersistence : ISqliteFilePersistence
{
    /// <inheritdoc />
    public Task RestoreIfExistsAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <inheritdoc />
    public Task PersistAsync(byte[] databaseBytes, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
