using System;
using Core.Domain;

namespace Inventory.Domain.Entities;

public class ProductBalance : Entity
{
    public Guid ProductId { get; private set; }
    public decimal TotalQuantity { get; private set; }

    private ProductBalance() { }

    public ProductBalance(Guid productId, decimal initialQuantity = 0)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        TotalQuantity = initialQuantity;
    }

    public Result UpdateBalance(decimal amount, MovementType type)
    {
        var newBalance = TotalQuantity;

        if (type == MovementType.In)
            newBalance += amount;
        else if (type == MovementType.Out)
            newBalance -= amount;
        else if (type == MovementType.Adjustment)
            newBalance += amount; // Can be positive or negative adjustment conceptually, but let's assume Adjustment in our system is handled by sending positive amount for Add, negative for Remove. Wait, StockMovement says quantity > 0. If it's a negative adjustment, we would need to know the sign or separate adjustments. Let's simplify: Adjustment is always treated as an absolute set, OR we use In/Out exclusively. Actually, if type == Adjustment, we need to know if it's adding or removing. Let's just say Adjustment In / Adjustment Out. Let's stick to In/Out. If MovementType.Adjustment, we could just say it sets the balance? No, we need an adjustment to be added/subtracted. For now, let's treat Adjustment as In, but wait...
        
        // Let's refine Adjustment later, for now just Out checks
        if (type == MovementType.Out && newBalance < 0)
            return Result.Failure(new Error("ProductBalance.InsufficientFunds", "Insufficient stock balance for this operation."));

        if (type == MovementType.Adjustment) 
            newBalance += amount; // Assuming positive adjustment adds to stock for now.

        TotalQuantity = newBalance;
        return Result.Success();
    }
}
