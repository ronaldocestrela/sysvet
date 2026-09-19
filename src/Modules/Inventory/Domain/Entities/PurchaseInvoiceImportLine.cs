using Core.Domain;

namespace Inventory.Domain.Entities;

/// <summary>
/// One import line (NF item or rastro split) pending or completed product mapping.
/// </summary>
public class PurchaseInvoiceImportLine : Entity
{
    public Guid PurchaseInvoiceImportId { get; private set; }
    public int ItemNumber { get; private set; }
    public string SupplierProductCode { get; private set; } = string.Empty;
    public string? Barcode { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string Ncm { get; private set; } = string.Empty;
    public string UnitOfMeasure { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal LineTotal { get; private set; }
    public string? LotNumber { get; private set; }
    public DateTimeOffset? ExpirationDate { get; private set; }
    public Guid? ProductId { get; private set; }
    public Guid? ProductLotId { get; private set; }
    public Guid? StockMovementId { get; private set; }

    private PurchaseInvoiceImportLine() { }

    /// <summary>
    /// Creates a line from parsed NF-e data.
    /// </summary>
    public static PurchaseInvoiceImportLine Create(
        Guid importId,
        int itemNumber,
        string supplierProductCode,
        string? barcode,
        string description,
        string ncm,
        string unitOfMeasure,
        decimal quantity,
        decimal unitCost,
        decimal lineTotal,
        string? lotNumber,
        DateTimeOffset? expirationDate,
        Guid? id = null)
    {
        return new PurchaseInvoiceImportLine
        {
            Id = id ?? Guid.NewGuid(),
            PurchaseInvoiceImportId = importId,
            ItemNumber = itemNumber,
            SupplierProductCode = supplierProductCode.Trim(),
            Barcode = string.IsNullOrWhiteSpace(barcode) ? null : barcode.Trim(),
            Description = description.Trim(),
            Ncm = ncm.Trim(),
            UnitOfMeasure = unitOfMeasure.Trim(),
            Quantity = quantity,
            UnitCost = unitCost,
            LineTotal = lineTotal,
            LotNumber = string.IsNullOrWhiteSpace(lotNumber) ? null : lotNumber.Trim().ToUpperInvariant(),
            ExpirationDate = expirationDate
        };
    }

    /// <summary>
    /// Assigns catalog product resolved during preview or confirm.
    /// </summary>
    public void AssignProduct(Guid productId) => ProductId = productId;

    /// <summary>
    /// Records lot and movement created on confirm for conference.
    /// </summary>
    public void MarkStockApplied(Guid? productLotId, Guid stockMovementId)
    {
        ProductLotId = productLotId;
        StockMovementId = stockMovementId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
