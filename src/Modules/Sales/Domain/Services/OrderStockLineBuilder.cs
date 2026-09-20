using Sales.Domain.Entities;
using Sales.Domain.Enums;

namespace Sales.Domain.Services;

/// <summary>Builds inventory debit lines from order items and kit definitions.</summary>
public static class OrderStockLineBuilder
{
    /// <summary>Aggregates product and exploded kit lines for stock consumption.</summary>
    public static IReadOnlyList<(Guid ProductId, decimal Quantity)> Build(
        IEnumerable<OrderItem> items,
        IReadOnlyDictionary<Guid, ProductKit> kitsById)
    {
        var totals = new Dictionary<Guid, decimal>();

        foreach (var item in items)
        {
            switch (item.Kind)
            {
                case OrderItemKind.Product when item.ProductId.HasValue:
                    Add(totals, item.ProductId.Value, item.Quantity);
                    break;
                case OrderItemKind.Kit when item.CatalogOfferId.HasValue &&
                                            kitsById.TryGetValue(item.CatalogOfferId.Value, out var kit):
                    foreach (var (productId, qty) in kit.ExplodeStockLines(item.Quantity))
                    {
                        Add(totals, productId, qty);
                    }

                    break;
            }
        }

        return totals.Select(kv => (kv.Key, kv.Value)).ToList();
    }

    private static void Add(Dictionary<Guid, decimal> totals, Guid productId, decimal quantity)
    {
        totals[productId] = totals.GetValueOrDefault(productId) + quantity;
    }
}
