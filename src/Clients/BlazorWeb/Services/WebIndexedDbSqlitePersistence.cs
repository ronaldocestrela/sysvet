using Clients.Infrastructure.Persistence;
using Microsoft.JSInterop;

namespace BlazorWeb.Services;

/// <summary>
/// Persists the WASM SQLite file to IndexedDB between browser sessions (ADR-014).
/// </summary>
public sealed class WebIndexedDbSqlitePersistence : ISqliteFilePersistence
{
    private readonly IJSRuntime _jsRuntime;

    /// <summary>Creates the persistence adapter.</summary>
    public WebIndexedDbSqlitePersistence(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <inheritdoc />
    public async Task RestoreIfExistsAsync(CancellationToken cancellationToken = default)
    {
        var bytes = await _jsRuntime.InvokeAsync<byte[]?>("sysvetDbStorage.get", cancellationToken);
        if (bytes is null or { Length: 0 })
        {
            return;
        }

        await File.WriteAllBytesAsync(SqliteFileHelper.DatabaseFileName, bytes, cancellationToken);
    }

    /// <inheritdoc />
    public Task PersistAsync(byte[] databaseBytes, CancellationToken cancellationToken = default)
        => _jsRuntime.InvokeVoidAsync("sysvetDbStorage.put", databaseBytes, cancellationToken).AsTask();
}
