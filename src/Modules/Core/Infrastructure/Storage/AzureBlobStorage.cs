using Azure.Storage.Blobs;
using Core.Application.Storage;
using Core.Domain;
using Core.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Core.Infrastructure.Storage;

/// <summary>Azure Blob Storage backend for production clinical attachments.</summary>
public sealed class AzureBlobStorage : IBlobStorage
{
    private readonly BlobContainerClient _container;

    /// <summary>Creates the Azure client from validated options.</summary>
    public AzureBlobStorage(IOptions<BlobStorageOptions> options)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.AzureConnectionString))
        {
            throw new InvalidOperationException("BlobStorage:AzureConnectionString is required when Provider is Azure.");
        }

        var service = new BlobServiceClient(settings.AzureConnectionString);
        _container = service.GetBlobContainerClient(settings.AzureContainer);
        _container.CreateIfNotExists();
    }

    /// <inheritdoc />
    public async Task<Result> PutAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var blob = _container.GetBlobClient(NormalizeKey(key));
        await blob.UploadAsync(content, overwrite: true, cancellationToken);
        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result<Stream>> OpenReadAsync(string key, CancellationToken cancellationToken = default)
    {
        var blob = _container.GetBlobClient(NormalizeKey(key));
        if (!await blob.ExistsAsync(cancellationToken))
        {
            return Result.Failure<Stream>(new Error("Blob.NotFound", "Blob object was not found."));
        }

        var response = await blob.DownloadStreamingAsync(cancellationToken: cancellationToken);
        return Result.Success(response.Value.Content);
    }

    /// <inheritdoc />
    public async Task<Result> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var blob = _container.GetBlobClient(NormalizeKey(key));
        await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        return Result.Success();
    }

    private static string NormalizeKey(string key) => key.Replace('\\', '/').TrimStart('/');
}
