using Core.Domain;

namespace Fiscal.Domain.Entities;

/// <summary>Immutable line snapshot on a fiscal document.</summary>
public sealed class FiscalDocumentItem : Entity
{
    public Guid FiscalDocumentId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string? Ncm { get; private set; }
    public string? Cfop { get; private set; }
    public string? Csosn { get; private set; }
    public int? MerchandiseOrigin { get; private set; }
    public Guid? ProductId { get; private set; }

    public decimal Total => Quantity * UnitPrice;

    private FiscalDocumentItem() { }

    internal static FiscalDocumentItem CreateProductLine(
        Guid documentId,
        Guid? productId,
        string description,
        decimal quantity,
        decimal unitPrice,
        string ncm,
        string cfop,
        string csosn,
        int merchandiseOrigin)
    {
        return new FiscalDocumentItem
        {
            Id = Guid.NewGuid(),
            FiscalDocumentId = documentId,
            ProductId = productId,
            Description = description,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Ncm = ncm,
            Cfop = cfop,
            Csosn = csosn,
            MerchandiseOrigin = merchandiseOrigin
        };
    }

    internal static FiscalDocumentItem CreateServiceLine(
        Guid documentId,
        string description,
        decimal quantity,
        decimal unitPrice)
    {
        return new FiscalDocumentItem
        {
            Id = Guid.NewGuid(),
            FiscalDocumentId = documentId,
            Description = description,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }
}
