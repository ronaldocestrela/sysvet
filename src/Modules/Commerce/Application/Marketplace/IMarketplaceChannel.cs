namespace Commerce.Application.Marketplace;

/// <summary>
/// Abstraction for marketplace listing and order integration (Mercado Livre PoC).
/// </summary>
public interface IMarketplaceChannel
{
    /// <summary>Channel identifier (e.g. MercadoLivre).</summary>
    string ChannelName { get; }

    /// <summary>Pushes or updates a listing with price and available quantity.</summary>
    Task<MarketplacePushResult> PushListingAsync(MarketplaceListingPush push, CancellationToken cancellationToken = default);

    /// <summary>Fetches remote order details for inbound webhook processing.</summary>
    Task<MarketplaceRemoteOrder?> GetOrderAsync(string externalOrderId, CancellationToken cancellationToken = default);
}

/// <summary>Listing push payload.</summary>
public sealed record MarketplaceListingPush(
    string Sku,
    string Title,
    decimal Price,
    decimal Quantity,
    string? ExternalListingId);

/// <summary>Result of a listing push.</summary>
public sealed record MarketplacePushResult(bool Success, string? ExternalListingId, string? Error);

/// <summary>Normalized remote marketplace order.</summary>
public sealed record MarketplaceRemoteOrder(
    string ExternalOrderId,
    string BuyerName,
    string BuyerPhone,
    string? BuyerEmail,
    IReadOnlyList<MarketplaceRemoteOrderLine> Lines);

/// <summary>Remote order line matched by SKU.</summary>
public sealed record MarketplaceRemoteOrderLine(string Sku, decimal Quantity, decimal UnitPrice);
