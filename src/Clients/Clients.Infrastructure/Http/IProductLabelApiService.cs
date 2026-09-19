using Core.Domain;

namespace Clients.Infrastructure.Http;

/// <summary>
/// Online API for product label downloads (PDF/ZPL).
/// </summary>
public interface IProductLabelApiService
{
    Task<Result<DownloadedFile>> DownloadProductLabelAsync(Guid productId, string format, int copies, CancellationToken cancellationToken = default);

    Task<Result<DownloadedFile>> DownloadLabelsAsync(IReadOnlyList<ProductLabelBatchItem> items, string format, CancellationToken cancellationToken = default);
}

/// <summary>Label batch line for POST /labels.</summary>
public sealed record ProductLabelBatchItem(Guid ProductId, int Copies);

/// <summary>Binary file downloaded from the API.</summary>
public sealed record DownloadedFile(byte[] Content, string ContentType, string FileName);
