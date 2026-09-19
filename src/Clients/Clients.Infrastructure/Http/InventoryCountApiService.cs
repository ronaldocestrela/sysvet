using Clients.Infrastructure.Sync;
using Core.Domain;

namespace Clients.Infrastructure.Http;

/// <summary>
/// REST client for inventory count endpoints.
/// </summary>
public sealed class InventoryCountApiService : IInventoryCountApiService
{
    private readonly ApiClient _apiClient;
    private readonly ISyncConnectivity _connectivity;

    public InventoryCountApiService(ApiClient apiClient, ISyncConnectivity connectivity)
    {
        _apiClient = apiClient;
        _connectivity = connectivity;
    }

    public Task<Result<Guid>> StartAsync(CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<Guid>(OfflineError()));
        }

        return _apiClient.PostAsync<object, Guid>("/api/v1/inventory/counts", new { }, cancellationToken: cancellationToken);
    }

    public Task<Result<IReadOnlyList<InventoryCountListItemClientDto>>> ListAsync(int take = 50, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<IReadOnlyList<InventoryCountListItemClientDto>>(OfflineError()));
        }

        return _apiClient.GetAsync<IReadOnlyList<InventoryCountListItemClientDto>>($"/api/v1/inventory/counts?take={take}", cancellationToken);
    }

    public Task<Result<InventoryCountDetailClientDto>> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<InventoryCountDetailClientDto>(OfflineError()));
        }

        return _apiClient.GetAsync<InventoryCountDetailClientDto>($"/api/v1/inventory/counts/{sessionId}", cancellationToken);
    }

    public Task<Result<Guid>> AddLineAsync(Guid sessionId, AddInventoryCountLineClientRequest request, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<Guid>(OfflineError()));
        }

        return _apiClient.PostAsync<AddInventoryCountLineClientRequest, Guid>(
            $"/api/v1/inventory/counts/{sessionId}/lines",
            request,
            cancellationToken: cancellationToken);
    }

    public Task<Result> SubmitAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure(OfflineError()));
        }

        return _apiClient.PostAsync<object>($"/api/v1/inventory/counts/{sessionId}/submit", new { }, cancellationToken: cancellationToken);
    }

    public Task<Result> ApproveAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure(OfflineError()));
        }

        return _apiClient.PostAsync<object>($"/api/v1/inventory/counts/{sessionId}/approve", new { }, cancellationToken: cancellationToken);
    }

    public Task<Result> CancelAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure(OfflineError()));
        }

        return _apiClient.PostAsync<object>($"/api/v1/inventory/counts/{sessionId}/cancel", new { }, cancellationToken: cancellationToken);
    }

    public async Task<Result<InventoryProductByBarcodeClientDto>> GetProductByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Result.Failure<InventoryProductByBarcodeClientDto>(OfflineError());
        }

        var encoded = Uri.EscapeDataString(barcode.Trim());
        var detail = await _apiClient.GetAsync<ProductDetailApiShape>($"/api/v1/inventory/products/by-barcode/{encoded}", cancellationToken);
        if (detail.IsFailure)
        {
            return Result.Failure<InventoryProductByBarcodeClientDto>(detail.Error);
        }

        return Result.Success(new InventoryProductByBarcodeClientDto
        {
            Id = detail.Value.Id,
            Name = detail.Value.Name,
            Sku = detail.Value.Sku,
            Barcode = detail.Value.Barcode,
            RequiresLot = detail.Value.RequiresLot,
            Lots = detail.Value.Lots.Select(l => new InventoryProductLotClientDto
            {
                Id = l.Id,
                LotNumber = l.LotNumber,
                Quantity = l.Quantity,
                IsActive = l.IsActive
            }).ToList()
        });
    }

    private static Error OfflineError() => new("InventoryCount.Offline", "Inventário requer conexão com a API.");

    private sealed class ProductDetailApiShape
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Sku { get; init; } = string.Empty;
        public string Barcode { get; init; } = string.Empty;
        public bool RequiresLot { get; init; }
        public List<ProductLotApiShape> Lots { get; init; } = new();
    }

    private sealed class ProductLotApiShape
    {
        public Guid Id { get; init; }
        public string LotNumber { get; init; } = string.Empty;
        public decimal Quantity { get; init; }
        public bool IsActive { get; init; }
    }
}
