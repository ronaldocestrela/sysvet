namespace Commerce.Application.Dtos;

/// <summary>Staff view of a product offer.</summary>
public sealed record ProductOfferDto(
    Guid Id,
    Guid ProductId,
    string Sku,
    string ProductName,
    decimal SalePrice,
    bool IsPublished,
    bool StoreEnabled,
    bool MercadoLivreEnabled,
    string? ExternalListingId,
    decimal AvailableQuantity);

/// <summary>Public storefront catalog item.</summary>
public sealed record PublicStoreProductDto(
    Guid OfferId,
    Guid ProductId,
    string Sku,
    string ProductName,
    decimal SalePrice,
    decimal AvailableQuantity,
    bool IsAvailable);

/// <summary>Public storefront order request line.</summary>
public sealed record PublicStoreOrderLineDto(Guid OfferId, decimal Quantity);

/// <summary>Staff online order summary.</summary>
public sealed record OnlineOrderDto(
    Guid Id,
    string Channel,
    string Status,
    string Fulfillment,
    string BuyerName,
    string BuyerPhone,
    string? BuyerEmail,
    decimal TotalAmount,
    DateTimeOffset? ConfirmedAt,
    IReadOnlyList<OnlineOrderLineDto> Lines);

/// <summary>Order line for staff API.</summary>
public sealed record OnlineOrderLineDto(
    Guid ProductId,
    Guid ProductOfferId,
    string ProductName,
    string Sku,
    decimal Quantity,
    decimal UnitPrice);

/// <summary>Mercado Livre integration status.</summary>
public sealed record MercadoLivreSettingsDto(
    bool IsEnabled,
    long? UserId,
    string SiteId,
    bool HasAccessToken);
