using Commerce.Domain.ValueObjects;
using Core.Domain;

namespace Commerce.Domain.Entities;

/// <summary>
/// Storefront listing for an inventory product with a retail sale price (ADR-045).
/// </summary>
public sealed class ProductOffer : AggregateRoot
{
    public Guid ProductId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public Money SalePrice { get; private set; } = Money.Zero;
    public bool IsPublished { get; private set; }
    public bool StoreEnabled { get; private set; }
    public bool MercadoLivreEnabled { get; private set; }
    public string? ExternalListingId { get; private set; }

#pragma warning disable CS8618
    private ProductOffer() : base(Guid.Empty) { }
#pragma warning restore CS8618

    private ProductOffer(Guid id, Guid productId, string sku, string productName, Money salePrice)
        : base(id)
    {
        ProductId = productId;
        Sku = sku;
        ProductName = productName;
        SalePrice = salePrice;
    }

    /// <summary>
    /// Creates a new offer for a catalog product.
    /// </summary>
    public static Result<ProductOffer> Create(Guid productId, string sku, string productName, decimal salePrice)
    {
        if (productId == Guid.Empty)
        {
            return Result.Failure<ProductOffer>(ErrorCodes.Offer.NotFound);
        }

        var price = Money.Create(salePrice);
        if (price.IsFailure)
        {
            return Result.Failure<ProductOffer>(price.Error);
        }

        if (price.Value.Amount <= 0)
        {
            return Result.Failure<ProductOffer>(ErrorCodes.Offer.InvalidPrice);
        }

        return Result.Success(new ProductOffer(
            Guid.NewGuid(),
            productId,
            sku.Trim(),
            productName.Trim(),
            price.Value));
    }

    /// <summary>
    /// Updates catalog snapshots and sale price from backoffice.
    /// </summary>
    public Result UpdateCatalogSnapshot(string sku, string productName, decimal salePrice)
    {
        var price = Money.Create(salePrice);
        if (price.IsFailure)
        {
            return price;
        }

        if (price.Value.Amount <= 0)
        {
            return Result.Failure(ErrorCodes.Offer.InvalidPrice);
        }

        Sku = sku.Trim();
        ProductName = productName.Trim();
        SalePrice = price.Value;
        return Result.Success();
    }

    /// <summary>Sets whether the offer is visible on channels.</summary>
    public void SetPublished(bool published) => IsPublished = published;

    /// <summary>Enables or disables the public clinic storefront.</summary>
    public void SetStoreEnabled(bool enabled) => StoreEnabled = enabled;

    /// <summary>Enables or disables Mercado Livre sync for this SKU.</summary>
    public void SetMercadoLivreEnabled(bool enabled) => MercadoLivreEnabled = enabled;

    /// <summary>Stores remote listing id after first push.</summary>
    public void SetExternalListingId(string? listingId) =>
        ExternalListingId = string.IsNullOrWhiteSpace(listingId) ? null : listingId.Trim();
}
