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
    public decimal Quantity { get; private set; }
    public string? BatchNumber { get; private set; }
    public DateTimeOffset? ExpirationDate { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTimeOffset Date { get; private set; }

    private StockMovement() { }

    private StockMovement(
        Guid productId,
        Guid? productLotId,
        MovementType type,
        decimal quantity,
        string? batchNumber,
        DateTimeOffset? expirationDate,
        string reason)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        ProductLotId = productLotId;
        Type = type;
        Quantity = quantity;
        BatchNumber = batchNumber;
        ExpirationDate = expirationDate;
        Reason = reason;
        Date = DateTimeOffset.UtcNow;
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
        Guid? productLotId = null)
    {
        if (quantity <= 0)
        {
            return Result.Failure<StockMovement>(ErrorCodes.StockMovement.InvalidQuantity);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure<StockMovement>(ErrorCodes.StockMovement.InvalidReason);
        }

        return Result.Success(new StockMovement(productId, productLotId, type, quantity, batchNumber, expirationDate, reason));
    }
}
