using Clients.Infrastructure.Http;
using Core.Domain;

namespace Clients.Infrastructure.Commerce;

/// <summary>Staff HTTP client for commerce offers and orders.</summary>
public sealed class CommerceApiService
{
    private readonly ApiClient _apiClient;

    public CommerceApiService(ApiClient apiClient) => _apiClient = apiClient;

    public Task<Result<IReadOnlyList<ProductOfferClientDto>>> ListOffersAsync(CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<IReadOnlyList<ProductOfferClientDto>>("/api/v1/commerce/offers", cancellationToken);

    public Task<Result<ProductOfferClientDto>> UpsertOfferAsync(UpsertProductOfferClientDto dto, CancellationToken cancellationToken = default) =>
        _apiClient.PutAsync<UpsertProductOfferClientDto, ProductOfferClientDto>("/api/v1/commerce/offers", dto, cancellationToken: cancellationToken);

    public Task<Result<IReadOnlyList<OnlineOrderClientDto>>> ListOrdersAsync(CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<IReadOnlyList<OnlineOrderClientDto>>("/api/v1/commerce/orders", cancellationToken);

    public Task<Result> MarkReadyAsync(Guid orderId, CancellationToken cancellationToken = default) =>
        _apiClient.PostAsync($"/api/v1/commerce/orders/{orderId}/ready", new { }, cancellationToken: cancellationToken);

    public Task<Result> CompleteAsync(Guid orderId, CancellationToken cancellationToken = default) =>
        _apiClient.PostAsync($"/api/v1/commerce/orders/{orderId}/complete", new { }, cancellationToken: cancellationToken);

    public Task<Result> CancelAsync(Guid orderId, CancellationToken cancellationToken = default) =>
        _apiClient.PostAsync($"/api/v1/commerce/orders/{orderId}/cancel", new { }, cancellationToken: cancellationToken);
}

/// <summary>Product offer DTO for staff UI.</summary>
public sealed class ProductOfferClientDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public bool IsPublished { get; set; }
    public bool StoreEnabled { get; set; }
    public bool MercadoLivreEnabled { get; set; }
    public decimal AvailableQuantity { get; set; }
}

/// <summary>Upsert offer request.</summary>
public sealed class UpsertProductOfferClientDto
{
    public Guid ProductId { get; set; }
    public decimal SalePrice { get; set; }
    public bool IsPublished { get; set; }
    public bool StoreEnabled { get; set; }
    public bool MercadoLivreEnabled { get; set; }
}

/// <summary>Online order DTO for staff UI.</summary>
public sealed class OnlineOrderClientDto
{
    public Guid Id { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerPhone { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public IReadOnlyList<OnlineOrderLineClientDto> Lines { get; set; } = [];
}

/// <summary>Order line DTO.</summary>
public sealed class OnlineOrderLineClientDto
{
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
