using Core.Domain;

namespace Core.Application.Storage;

/// <summary>
/// Abstraction for tenant-scoped binary object storage (clinical attachments, exports, etc.).
/// </summary>
public interface IBlobStorage
{
    /// <summary>Stores or overwrites an object at the given key.</summary>
    Task<Result> PutAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default);

    /// <summary>Opens a read stream for an existing object.</summary>
    Task<Result<Stream>> OpenReadAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Deletes an object if present (idempotent).</summary>
    Task<Result> DeleteAsync(string key, CancellationToken cancellationToken = default);
}
