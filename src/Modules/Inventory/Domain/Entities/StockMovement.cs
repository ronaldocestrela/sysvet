using Core.Domain;
using Inventory.Domain;

namespace Inventory.Domain.Entities;

public class StockMovement : Entity
{
    public Guid ProductId { get; private set; }
    public MovementType Type { get; private set; }
    public decimal Quantity { get; private set; }
    public string? BatchNumber { get; private set; }
    public DateTimeOffset? ExpirationDate { get; private set; }
    public string Reason { get; private set; }
    public DateTimeOffset Date { get; private set; }

    private StockMovement(Guid productId, MovementType type, decimal quantity, string? batchNumber, DateTimeOffset? expirationDate, string reason)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        Type = type;
        Quantity = quantity;
        BatchNumber = batchNumber;
        ExpirationDate = expirationDate;
        Reason = reason;
        Date = DateTimeOffset.UtcNow;
    }

    public static Result<StockMovement> Create(Guid productId, MovementType type, decimal quantity, string? batchNumber, DateTimeOffset? expirationDate, string reason)
    {
        if (quantity <= 0)
            return Result.Failure<StockMovement>(ErrorCodes.StockMovement.InvalidQuantity);

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure<StockMovement>(ErrorCodes.StockMovement.InvalidReason);

        return Result.Success(new StockMovement(productId, type, quantity, batchNumber, expirationDate, reason));
    }
}
