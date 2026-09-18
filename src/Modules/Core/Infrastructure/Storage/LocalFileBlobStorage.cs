using Core.Application.Storage;
using Core.Domain;
using Core.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Core.Infrastructure.Storage;

/// <summary>Filesystem blob storage for development and integration tests.</summary>
public sealed class LocalFileBlobStorage : IBlobStorage
{
    private readonly string _rootPath;

    /// <summary>Creates storage rooted at configured path.</summary>
    public LocalFileBlobStorage(IOptions<BlobStorageOptions> options)
    {
        _rootPath = Path.GetFullPath(options.Value.LocalRootPath);
        Directory.CreateDirectory(_rootPath);
    }

    /// <inheritdoc />
    public async Task<Result> PutAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var file = File.Create(path);
        await content.CopyToAsync(file, cancellationToken);
        return Result.Success();
    }

    /// <inheritdoc />
    public Task<Result<Stream>> OpenReadAsync(string key, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(key);
        if (!File.Exists(path))
        {
            return Task.FromResult(Result.Failure<Stream>(new Error("Blob.NotFound", "Blob object was not found.")));
        }

        Stream stream = File.OpenRead(path);
        return Task.FromResult(Result.Success(stream));
    }

    /// <inheritdoc />
    public Task<Result> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(key);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.FromResult(Result.Success());
    }

    private string ResolvePath(string key)
    {
        var normalized = key.Replace('\\', '/').TrimStart('/');
        var combined = Path.GetFullPath(Path.Combine(_rootPath, normalized));
        if (!combined.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid blob key path traversal.");
        }

        return combined;
    }
}
