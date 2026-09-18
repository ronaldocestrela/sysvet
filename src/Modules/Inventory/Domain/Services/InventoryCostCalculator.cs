namespace Inventory.Domain.Services;

/// <summary>
/// Pure functions for inventory costing and balance projection from lots.
/// </summary>
public static class InventoryCostCalculator
{
    /// <summary>
    /// Weighted average unit cost from active lots with positive quantity.
    /// </summary>
    public static decimal WeightedAverageCost(IEnumerable<(decimal Quantity, decimal UnitCost)> activeLots)
    {
        decimal totalQty = 0;
        decimal totalValue = 0;
        foreach (var (quantity, unitCost) in activeLots)
        {
            if (quantity <= 0)
            {
                continue;
            }

            totalQty += quantity;
            totalValue += quantity * unitCost;
        }

        return totalQty <= 0 ? 0m : decimal.Round(totalValue / totalQty, 4, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Sums quantities from active lots only.
    /// </summary>
    public static decimal TotalQuantityFromLots(IEnumerable<(decimal Quantity, bool IsActive)> lots)
    {
        return lots.Where(l => l.IsActive).Sum(l => l.Quantity);
    }
}
