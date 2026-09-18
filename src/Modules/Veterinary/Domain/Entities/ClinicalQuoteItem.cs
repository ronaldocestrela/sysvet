using Core.Domain;
using Veterinary.Domain.Enums;

namespace Veterinary.Domain.Entities;

/// <summary>Snapshot line on a clinical quote (price captured at quote time).</summary>
public sealed class ClinicalQuoteItem : Entity
{
    /// <summary>Parent quote identifier.</summary>
    public Guid ClinicalQuoteId { get; private set; }

    /// <summary>Human-readable line description.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Quantity (supports fractional units).</summary>
    public decimal Quantity { get; private set; }

    /// <summary>Unit price in BRL at quote time.</summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>Product vs service classification.</summary>
    public ClinicalQuoteItemKind Kind { get; private set; }

    /// <summary>Optional inventory product reference (no cross-module FK).</summary>
    public Guid? ProductId { get; private set; }

    /// <summary>Display order.</summary>
    public int SortOrder { get; private set; }

    private ClinicalQuoteItem() { }

    private ClinicalQuoteItem(
        Guid id,
        Guid clinicalQuoteId,
        string description,
        decimal quantity,
        decimal unitPrice,
        ClinicalQuoteItemKind kind,
        Guid? productId,
        int sortOrder)
        : base(id)
    {
        ClinicalQuoteId = clinicalQuoteId;
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Kind = kind;
        ProductId = productId;
        SortOrder = sortOrder;
    }

    /// <summary>Creates a validated quote line.</summary>
    public static Result<ClinicalQuoteItem> Create(
        Guid id,
        Guid clinicalQuoteId,
        string description,
        decimal quantity,
        decimal unitPrice,
        ClinicalQuoteItemKind kind,
        Guid? productId,
        int sortOrder)
    {
        if (clinicalQuoteId == Guid.Empty)
        {
            return Result.Failure<ClinicalQuoteItem>(ErrorCodes.ClinicalQuote.InvalidIdentifiers);
        }

        if (string.IsNullOrWhiteSpace(description) || description.Length > 500)
        {
            return Result.Failure<ClinicalQuoteItem>(ErrorCodes.ClinicalQuote.InvalidDescription);
        }

        if (quantity <= 0)
        {
            return Result.Failure<ClinicalQuoteItem>(ErrorCodes.ClinicalQuote.InvalidQuantity);
        }

        if (unitPrice < 0)
        {
            return Result.Failure<ClinicalQuoteItem>(ErrorCodes.ClinicalQuote.InvalidPrice);
        }

        return Result.Success(new ClinicalQuoteItem(
            id,
            clinicalQuoteId,
            description.Trim(),
            quantity,
            unitPrice,
            kind,
            productId == Guid.Empty ? null : productId,
            sortOrder));
    }

    /// <summary>Line total in BRL.</summary>
    public decimal LineTotal => Quantity * UnitPrice;
}
