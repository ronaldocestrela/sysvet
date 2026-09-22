using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Commerce.Application.Marketplace;
using Commerce.Domain.Repositories;
using Commerce.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Commerce.Infrastructure.Marketplace;

/// <summary>Thin Mercado Livre REST adapter (manual access token PoC).</summary>
public sealed class MercadoLivreChannel : IMarketplaceChannel
{
    private readonly HttpClient _httpClient;
    private readonly IMercadoLivreSettingsRepository _settingsRepository;
    private readonly IOptions<CommerceOptions> _options;
    private readonly ILogger<MercadoLivreChannel> _logger;

    public MercadoLivreChannel(
        HttpClient httpClient,
        IMercadoLivreSettingsRepository settingsRepository,
        IOptions<CommerceOptions> options,
        ILogger<MercadoLivreChannel> logger)
    {
        _httpClient = httpClient;
        _settingsRepository = settingsRepository;
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc />
    public string ChannelName => "MercadoLivre";

    /// <inheritdoc />
    public async Task<MarketplacePushResult> PushListingAsync(MarketplaceListingPush push, CancellationToken cancellationToken = default)
    {
        var settings = await _settingsRepository.GetAsync(cancellationToken);
        if (settings is null || !settings.IsEnabled || string.IsNullOrWhiteSpace(settings.AccessToken))
        {
            return new MarketplacePushResult(false, push.ExternalListingId, "Mercado Livre not configured");
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, BuildListingUrl(settings, push.ExternalListingId));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.AccessToken);
            request.Content = JsonContent.Create(new
            {
                price = push.Price,
                available_quantity = (int)Math.Floor(push.Quantity)
            });

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Mercado Livre push failed: {Status} {Body}", response.StatusCode, body);
                return new MarketplacePushResult(false, push.ExternalListingId, body);
            }

            return new MarketplacePushResult(true, push.ExternalListingId ?? push.Sku, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mercado Livre push exception");
            return new MarketplacePushResult(false, push.ExternalListingId, ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task<MarketplaceRemoteOrder?> GetOrderAsync(string externalOrderId, CancellationToken cancellationToken = default)
    {
        var settings = await _settingsRepository.GetAsync(cancellationToken);
        if (settings is null || !settings.IsEnabled || string.IsNullOrWhiteSpace(settings.AccessToken))
        {
            return null;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{_options.Value.MercadoLivreApiBaseUrl}/orders/{externalOrderId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.AccessToken);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var root = doc.RootElement;
            var buyerName = root.GetProperty("buyer").GetProperty("nickname").GetString() ?? "ML Buyer";
            var lines = new List<MarketplaceRemoteOrderLine>();
            if (root.TryGetProperty("order_items", out var items))
            {
                foreach (var item in items.EnumerateArray())
                {
                    var sku = item.GetProperty("item").GetProperty("seller_sku").GetString() ?? string.Empty;
                    var qty = item.GetProperty("quantity").GetDecimal();
                    var unitPrice = item.GetProperty("unit_price").GetDecimal();
                    lines.Add(new MarketplaceRemoteOrderLine(sku, qty, unitPrice));
                }
            }

            return new MarketplaceRemoteOrder(
                externalOrderId,
                buyerName,
                "0000000000",
                null,
                lines);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mercado Livre get order failed");
            return null;
        }
    }

    private static string BuildListingUrl(Domain.Entities.MercadoLivreSettings settings, string? listingId) =>
        string.IsNullOrWhiteSpace(listingId)
            ? "https://api.mercadolibre.com/items"
            : $"https://api.mercadolibre.com/items/{listingId}";
}
