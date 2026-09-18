using System.Collections.Concurrent;
using Core.Application.Storage;
using Core.Domain;

namespace Core.Infrastructure.Storage;

/// <summary>In-memory blob storage for unit tests.</summary>
public sealed class InMemoryBlobStorage : IBlobStorage
{
    private readonly ConcurrentDictionary<string, byte[]> _store = new();

    /// <inheritdoc />
    public async Task<Result> PutAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        using var ms = new MemoryStream();
        await content.CopyToAsync(ms, cancellationToken);
        _store[key] = ms.ToArray();
        return Result.Success();
    }

    /// <inheritdoc />
    public Task<Result<Stream>> OpenReadAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!_store.TryGetValue(key, out var bytes))
        {
            return Task.FromResult(Result.Failure<Stream>(new Error("Blob.NotFound", "Blob object was not found.")));
        }

        return Task.FromResult(Result.Success<Stream>(new MemoryStream(bytes, writable: false)));
    }

    /// <inheritdoc />
    public Task<Result> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        _store.TryRemove(key, out _);
        return Task.FromResult(Result.Success());
    }
}
