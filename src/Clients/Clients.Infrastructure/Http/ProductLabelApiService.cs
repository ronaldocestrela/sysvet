using Clients.Infrastructure.Sync;
using Core.Domain;

namespace Clients.Infrastructure.Http;

/// <summary>
/// REST client for inventory label endpoints.
/// </summary>
public sealed class ProductLabelApiService : IProductLabelApiService
{
    private readonly ApiClient _apiClient;
    private readonly ISyncConnectivity _connectivity;

    public ProductLabelApiService(ApiClient apiClient, ISyncConnectivity connectivity)
    {
        _apiClient = apiClient;
        _connectivity = connectivity;
    }

    public Task<Result<DownloadedFile>> DownloadProductLabelAsync(Guid productId, string format, int copies, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<DownloadedFile>(OfflineError()));
        }

        var fmt = string.IsNullOrWhiteSpace(format) ? "pdf" : format;
        var copyCount = copies <= 0 ? 1 : copies;
        return _apiClient.DownloadGetAsync($"/api/v1/inventory/products/{productId}/label?format={fmt}&copies={copyCount}", cancellationToken);
    }

    public Task<Result<DownloadedFile>> DownloadLabelsAsync(IReadOnlyList<ProductLabelBatchItem> items, string format, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<DownloadedFile>(OfflineError()));
        }

        var body = new
        {
            Format = string.IsNullOrWhiteSpace(format) ? "pdf" : format,
            Items = items.Select(i => new { ProductId = i.ProductId, Copies = i.Copies <= 0 ? 1 : i.Copies }).ToList()
        };

        return _apiClient.DownloadPostAsync("/api/v1/inventory/labels", body, cancellationToken);
    }

    private static Error OfflineError() => new("Sync.Offline", "Etiquetas exigem conexão com a API.");
}
