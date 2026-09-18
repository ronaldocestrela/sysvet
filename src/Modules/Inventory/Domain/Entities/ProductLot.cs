using Core.Domain;
using Inventory.Domain;
using Inventory.Domain.ValueObjects;

namespace Inventory.Domain.Entities;

/// <summary>
/// Batch/lot with on-hand quantity, expiry and unit cost for a product.
/// </summary>
public class ProductLot : Entity
{
    public Guid ProductId { get; private set; }
    public string LotNumber { get; private set; } = string.Empty;
    public DateTimeOffset? ExpirationDate { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal Quantity { get; private set; }
    public bool IsActive { get; private set; } = true;

    private ProductLot() { }

    private ProductLot(Guid id, Guid productId, string lotNumber, DateTimeOffset? expirationDate, decimal unitCost, decimal quantity)
        : base(id)
    {
        ProductId = productId;
        LotNumber = lotNumber;
        ExpirationDate = expirationDate;
        UnitCost = unitCost;
        Quantity = quantity;
    }

    /// <summary>
    /// Registers a new lot with optional opening quantity.
    /// </summary>
    public static Result<ProductLot> Create(
        Guid productId,
        string lotNumber,
        DateTimeOffset? expirationDate,
        decimal unitCost,
        decimal initialQuantity,
        Guid? id = null)
    {
        if (productId == Guid.Empty)
        {
            return Result.Failure<ProductLot>(ErrorCodes.ProductLot.InvalidProduct);
        }

        if (string.IsNullOrWhiteSpace(lotNumber))
        {
            return Result.Failure<ProductLot>(ErrorCodes.ProductLot.InvalidLotNumber);
        }

        if (unitCost < 0)
        {
            return Result.Failure<ProductLot>(ErrorCodes.ProductLot.InvalidUnitCost);
        }

        if (initialQuantity < 0)
        {
            return Result.Failure<ProductLot>(ErrorCodes.ProductLot.InvalidQuantity);
        }

        var lotId = id ?? Guid.NewGuid();
        return Result.Success(new ProductLot(
            lotId,
            productId,
            lotNumber.Trim().ToUpperInvariant(),
            expirationDate,
            unitCost,
            initialQuantity));
    }

    /// <summary>
    /// Updates metadata without changing on-hand quantity.
    /// </summary>
    public Result UpdateMetadata(DateTimeOffset? expirationDate, decimal unitCost)
    {
        if (unitCost < 0)
        {
            return Result.Failure(ErrorCodes.ProductLot.InvalidUnitCost);
        }

        ExpirationDate = expirationDate;
        UnitCost = unitCost;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Applies a signed quantity delta; rejects negative balance.
    /// </summary>
    public Result AdjustQuantity(decimal delta)
    {
        var newQty = Quantity + delta;
        if (newQty < 0)
        {
            return Result.Failure(ErrorCodes.ProductLot.InsufficientQuantity);
        }

        Quantity = newQty;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Sets quantity directly (sync/reconciliation).
    /// </summary>
    public Result SetQuantity(decimal quantity)
    {
        if (quantity < 0)
        {
            return Result.Failure(ErrorCodes.ProductLot.InvalidQuantity);
        }

        Quantity = quantity;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Deactivates lot from balance and costing projections.
    /// </summary>
    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
