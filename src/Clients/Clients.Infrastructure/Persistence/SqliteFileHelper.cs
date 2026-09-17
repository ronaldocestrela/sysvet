using Microsoft.EntityFrameworkCore;

namespace Clients.Infrastructure.Persistence;

/// <summary>
/// Reads SQLite files from the connection and applies WAL flush before IndexedDB snapshots.
/// </summary>
public static class SqliteFileHelper
{
    /// <summary>
    /// Default database file name used by Blazor WASM MEMFS and MAUI AppDataDirectory.
    /// </summary>
    public const string DatabaseFileName = "sysvet.db";

    /// <summary>
    /// Ensures WAL is checkpointed and persists the database file when a persistence adapter is configured.
    /// </summary>
    public static async Task FlushToPersistentStorageAsync(
        OfflineDbContext context,
        ISqliteFilePersistence persistence,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await context.Database.ExecuteSqlRawAsync("PRAGMA wal_checkpoint(FULL);", cancellationToken);
            await context.Database.ExecuteSqlRawAsync("PRAGMA synchronous = FULL;", cancellationToken);
        }
        catch
        {
            // WAL may be unavailable on in-memory providers; persistence still attempts a file read.
        }

        var connectionString = context.Database.GetConnectionString() ?? $"Data Source={DatabaseFileName}";
        var filePath = ResolveFilePath(connectionString);

        if (!File.Exists(filePath))
        {
            return;
        }

        var bytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
        await persistence.PersistAsync(bytes, cancellationToken);
    }

    /// <summary>
    /// Parses the file path from a SQLite connection string (e.g. <c>Data Source=sysvet.db</c>).
    /// </summary>
    public static string ResolveFilePath(string connectionString)
    {
        const string prefix = "Data Source=";
        var idx = connectionString.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
        {
            return DatabaseFileName;
        }

        var path = connectionString[(idx + prefix.Length)..].Trim();
        var semi = path.IndexOf(';');
        if (semi >= 0)
        {
            path = path[..semi];
        }

        return string.IsNullOrWhiteSpace(path) ? DatabaseFileName : path;
    }
}
