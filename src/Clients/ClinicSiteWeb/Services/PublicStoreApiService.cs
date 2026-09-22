using System.Net.Http.Json;

namespace ClinicSiteWeb.Services;

/// <summary>Anonymous storefront API for ClinicSiteWeb.</summary>
public sealed class PublicStoreApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public PublicStoreApiService(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public async Task<IReadOnlyList<PublicStoreProductClientDto>> GetCatalogAsync(string slug, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("PublicApi");
        var list = await client.GetFromJsonAsync<List<PublicStoreProductClientDto>>(
            $"api/v1/public/clinic-sites/{Uri.EscapeDataString(slug)}/store/catalog",
            cancellationToken);
        return list ?? [];
    }

    public async Task<PublicStoreProductClientDto?> GetProductAsync(string slug, Guid offerId, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("PublicApi");
        return await client.GetFromJsonAsync<PublicStoreProductClientDto>(
            $"api/v1/public/clinic-sites/{Uri.EscapeDataString(slug)}/store/products/{offerId}",
            cancellationToken);
    }

    public async Task<bool> PlaceOrderAsync(string slug, PlaceStoreOrderClientDto order, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("PublicApi");
        var response = await client.PostAsJsonAsync(
            $"api/v1/public/clinic-sites/{Uri.EscapeDataString(slug)}/store/orders",
            order,
            cancellationToken);
        return response.IsSuccessStatusCode;
    }
}

public sealed class PublicStoreProductClientDto
{
    public Guid OfferId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public decimal AvailableQuantity { get; set; }
    public bool IsAvailable { get; set; }
}

public sealed class PlaceStoreOrderClientDto
{
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerPhone { get; set; } = string.Empty;
    public string? BuyerEmail { get; set; }
    public List<PlaceStoreOrderLineClientDto> Lines { get; set; } = [];
}

public sealed class PlaceStoreOrderLineClientDto
{
    public Guid OfferId { get; set; }
    public decimal Quantity { get; set; }
}
