using Inventory.Domain.Entities;
using Inventory.Domain.Services;

namespace Inventory.Application.Common;

/// <summary>
/// Reconciles product balance and average cost from lot rows.
/// </summary>
public static class ProductCatalogProjection
{
    /// <summary>
    /// Updates balance and product average cost from active lots.
    /// </summary>
    public static void ApplyLotSnapshot(Product product, ProductBalance balance, IEnumerable<ProductLot> lots)
    {
        var activeLots = lots.Where(l => l.IsActive).ToList();
        var total = InventoryCostCalculator.TotalQuantityFromLots(activeLots.Select(l => (l.Quantity, l.IsActive)));
        balance.SyncFromLots(total);
        var avg = InventoryCostCalculator.WeightedAverageCost(
            activeLots.Select(l => (l.Quantity, l.UnitCost)));
        product.RecalculateAverageCost(avg);
    }
}
