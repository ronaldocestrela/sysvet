using Core.Domain;

namespace Inventory.Domain.Entities;

/// <summary>
/// One counted product or lot line within an inventory session.
/// </summary>
public class InventoryCountLine : Entity
{
    public Guid InventoryCountId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? ProductLotId { get; private set; }
    public decimal CountedQuantity { get; private set; }
    public decimal? ExpectedQuantity { get; private set; }
    public decimal? Variance { get; private set; }
    public Guid? StockMovementId { get; private set; }

    private InventoryCountLine() { }

    /// <summary>
    /// Creates a new count line for a product or lot.
    /// </summary>
    public static InventoryCountLine Create(
        Guid inventoryCountId,
        Guid productId,
        Guid? productLotId,
        decimal initialCountedQuantity,
        Guid? id = null)
    {
        return new InventoryCountLine
        {
            Id = id ?? Guid.NewGuid(),
            InventoryCountId = inventoryCountId,
            ProductId = productId,
            ProductLotId = productLotId,
            CountedQuantity = initialCountedQuantity
        };
    }

    /// <summary>
    /// Adds to the counted quantity (barcode rescan).
    /// </summary>
    internal void IncrementCounted(decimal quantity)
    {
        CountedQuantity += quantity;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Replaces the counted quantity during blind count editing.
    /// </summary>
    internal void SetCountedQuantity(decimal quantity)
    {
        CountedQuantity = quantity;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Snapshots system on-hand at submit time.
    /// </summary>
    internal void ApplySubmitSnapshot(decimal expectedQuantity)
    {
        ExpectedQuantity = expectedQuantity;
        Variance = CountedQuantity - expectedQuantity;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Links the adjustment movement created on approve.
    /// </summary>
    public void MarkStockMovementApplied(Guid stockMovementId)
    {
        StockMovementId = stockMovementId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
