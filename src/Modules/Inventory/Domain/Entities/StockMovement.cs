using Core.Domain;
using Inventory.Domain;

namespace Inventory.Domain.Entities;

/// <summary>
/// Immutable stock ledger entry for a product (optionally tied to a lot).
/// </summary>
public class StockMovement : Entity
{
    public Guid ProductId { get; private set; }
    public Guid? ProductLotId { get; private set; }
    public MovementType Type { get; private set; }
    public AdjustmentDirection? AdjustmentDirection { get; private set; }
    public decimal Quantity { get; private set; }
    public string? BatchNumber { get; private set; }
    public DateTimeOffset? ExpirationDate { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public Guid? SupplierId { get; private set; }
    public DateTimeOffset Date { get; private set; }
    public Guid? CorrelationId { get; private set; }

    private StockMovement() { }

    private StockMovement(
        Guid id,
        Guid productId,
        Guid? productLotId,
        MovementType type,
        AdjustmentDirection? adjustmentDirection,
        decimal quantity,
        string? batchNumber,
        DateTimeOffset? expirationDate,
        string reason,
        string? notes,
        Guid? supplierId,
        DateTimeOffset date,
        Guid? correlationId)
        : base(id)
    {
        ProductId = productId;
        ProductLotId = productLotId;
        Type = type;
        AdjustmentDirection = adjustmentDirection;
        Quantity = quantity;
        BatchNumber = batchNumber;
        ExpirationDate = expirationDate;
        Reason = reason;
        Notes = notes;
        SupplierId = supplierId;
        Date = date;
        CorrelationId = correlationId;
    }

    /// <summary>
    /// Creates a validated movement line.
    /// </summary>
    public static Result<StockMovement> Create(
        Guid productId,
        MovementType type,
        decimal quantity,
        string? batchNumber,
        DateTimeOffset? expirationDate,
        string reason,
        Guid? productLotId = null,
        AdjustmentDirection? adjustmentDirection = null,
        Guid? id = null,
        DateTimeOffset? occurredAt = null,
        Guid? correlationId = null,
        Guid? supplierId = null,
        string? notes = null)
    {
        if (quantity <= 0)
        {
            return Result.Failure<StockMovement>(ErrorCodes.StockMovement.InvalidQuantity);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure<StockMovement>(ErrorCodes.StockMovement.InvalidReason);
        }

        if (type == MovementType.Adjustment && adjustmentDirection is null)
        {
            return Result.Failure<StockMovement>(ErrorCodes.StockMovement.InvalidAdjustmentDirection);
        }

        if (type != MovementType.Adjustment && adjustmentDirection is not null)
        {
            return Result.Failure<StockMovement>(ErrorCodes.StockMovement.InvalidAdjustmentDirection);
        }

        var movementId = id ?? Guid.NewGuid();
        var date = occurredAt ?? DateTimeOffset.UtcNow;

        return Result.Success(new StockMovement(
            movementId,
            productId,
            productLotId,
            type,
            adjustmentDirection,
            quantity,
            batchNumber,
            expirationDate,
            reason,
            notes,
            supplierId,
            date,
            correlationId));
    }
}
