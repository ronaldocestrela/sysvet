using Inventory.Domain.Entities;

namespace Inventory.Domain.Services;

/// <summary>
/// FEFO allocation: earliest expiry first, null expiry last.
/// </summary>
public static class LotAllocationService
{
    /// <summary>
    /// Orders active lots with quantity for consumption (sales auto-allocation).
    /// </summary>
    public static IReadOnlyList<ProductLot> OrderForConsumption(IEnumerable<ProductLot> lots) =>
        lots
            .Where(l => l.IsActive && l.Quantity > 0)
            .OrderBy(l => l.ExpirationDate.HasValue ? 0 : 1)
            .ThenBy(l => l.ExpirationDate ?? DateTimeOffset.MaxValue)
            .ThenBy(l => l.LotNumber, StringComparer.Ordinal)
            .ToList();

    /// <summary>
    /// Allocates quantity across FEFO-ordered lots; returns pairs of lot and quantity taken.
    /// </summary>
    public static IReadOnlyList<(ProductLot Lot, decimal Quantity)> Allocate(
        IEnumerable<ProductLot> lots,
        decimal quantityNeeded)
    {
        if (quantityNeeded <= 0)
        {
            return Array.Empty<(ProductLot, decimal)>();
        }

        var remaining = quantityNeeded;
        var result = new List<(ProductLot, decimal)>();

        foreach (var lot in OrderForConsumption(lots))
        {
            if (remaining <= 0)
            {
                break;
            }

            var take = Math.Min(lot.Quantity, remaining);
            if (take <= 0)
            {
                continue;
            }

            result.Add((lot, take));
            remaining -= take;
        }

        return remaining > 0 ? Array.Empty<(ProductLot, decimal)>() : result;
    }
}
