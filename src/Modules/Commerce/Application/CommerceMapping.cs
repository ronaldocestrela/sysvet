using Commerce.Application.Dtos;
using Commerce.Domain.Entities;
using Commerce.Domain.Enums;

namespace Commerce.Application;

/// <summary>Maps commerce aggregates to API DTOs.</summary>
public static class CommerceMapping
{
    /// <summary>Maps offer to staff DTO with optional stock.</summary>
    public static ProductOfferDto ToDto(ProductOffer offer, decimal availableQuantity) =>
        new(
            offer.Id,
            offer.ProductId,
            offer.Sku,
            offer.ProductName,
            offer.SalePrice.Amount,
            offer.IsPublished,
            offer.StoreEnabled,
            offer.MercadoLivreEnabled,
            offer.ExternalListingId,
            availableQuantity);

    /// <summary>Maps offer to public catalog DTO.</summary>
    public static PublicStoreProductDto ToPublicDto(ProductOffer offer, decimal availableQuantity) =>
        new(
            offer.Id,
            offer.ProductId,
            offer.Sku,
            offer.ProductName,
            offer.SalePrice.Amount,
            availableQuantity,
            availableQuantity > 0);

    /// <summary>Maps online order to staff DTO.</summary>
    public static OnlineOrderDto ToDto(OnlineOrder order) =>
        new(
            order.Id,
            order.Channel.ToString(),
            order.Status.ToString(),
            order.Fulfillment.ToString(),
            order.BuyerName,
            order.BuyerPhone,
            order.BuyerEmail,
            order.TotalAmount,
            order.ConfirmedAt,
            order.Lines.Select(l => new OnlineOrderLineDto(
                l.ProductId,
                l.ProductOfferId,
                l.ProductName,
                l.Sku,
                l.Quantity,
                l.UnitPrice.Amount)).ToList());
}
