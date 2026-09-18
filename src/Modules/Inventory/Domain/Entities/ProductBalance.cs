using Core.Domain;
using Inventory.Domain;

namespace Inventory.Domain.Entities;

/// <summary>
/// Projected total on-hand quantity for a product (sum of active lots).
/// </summary>
public class ProductBalance : Entity
{
    public Guid ProductId { get; private set; }
    public decimal TotalQuantity { get; private set; }

    private ProductBalance() { }

    /// <summary>
    /// Creates balance row for a new product.
    /// </summary>
    public ProductBalance(Guid productId, decimal initialQuantity = 0)
        : base(Guid.NewGuid())
    {
        ProductId = productId;
        TotalQuantity = initialQuantity;
    }

    /// <summary>
    /// Sets total from lot aggregation.
    /// </summary>
    public void SyncFromLots(decimal totalFromLots)
    {
        if (totalFromLots < 0)
        {
            throw new InvalidOperationException("Total quantity cannot be negative.");
        }

        TotalQuantity = totalFromLots;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Applies movement delta (legacy path until 5.2 lot-aware movements).
    /// </summary>
    public Result UpdateBalance(decimal amount, MovementType type)
    {
        var newBalance = TotalQuantity;

        if (type == MovementType.In)
        {
            newBalance += amount;
        }
        else if (type == MovementType.Out)
        {
            newBalance -= amount;
        }
        else if (type == MovementType.Adjustment)
        {
            newBalance += amount;
        }

        if (type == MovementType.Out && newBalance < 0)
        {
            return Result.Failure(ErrorCodes.ProductBalance.InsufficientFunds);
        }

        TotalQuantity = newBalance;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
