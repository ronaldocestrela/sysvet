namespace Inventory.Domain.Entities;

/// <summary>
/// Signed effect for adjustment movements (quantity is always positive).
/// </summary>
public enum AdjustmentDirection
{
    Increase = 0,
    Decrease = 1
}
