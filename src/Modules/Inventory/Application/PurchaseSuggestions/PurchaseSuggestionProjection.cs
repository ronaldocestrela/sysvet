using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using Inventory.Domain.Services;

namespace Inventory.Application.PurchaseSuggestions;

/// <summary>
/// Builds purchase suggestion groups from catalog and on-hand snapshots.
/// </summary>
public static class PurchaseSuggestionProjection
{
    /// <summary>
    /// Computes supplier-grouped suggestion lines for active products below reorder level.
    /// </summary>
    public static async Task<IReadOnlyList<PurchaseSuggestionGroupDto>> BuildAsync(
        IProductRepository productRepository,
        IProductLotRepository lotRepository,
        ISupplierRepository supplierRepository,
        Guid? supplierFilter,
        CancellationToken cancellationToken)
    {
        var products = (await productRepository.GetAllAsync(cancellationToken)).Where(p => p.IsActive).ToList();
        var suppliers = (await supplierRepository.GetAllAsync(cancellationToken)).ToDictionary(s => s.Id);
        var linesBySupplier = new Dictionary<string, List<PurchaseSuggestionLineDto>>(StringComparer.Ordinal);

        foreach (var product in products)
        {
            if (supplierFilter is Guid filter && product.SupplierId != filter)
            {
                continue;
            }

            var onHand = await ResolveOnHandAsync(productRepository, lotRepository, product.Id, cancellationToken);
            var suggested = PurchaseSuggestionCalculator.ComputeSuggestedQuantity(
                onHand,
                product.ReorderLevel,
                product.TargetStock,
                product.UnitsPerPackage);
            if (suggested is null)
            {
                continue;
            }

            var key = product.SupplierId?.ToString() ?? string.Empty;
            if (!linesBySupplier.TryGetValue(key, out var list))
            {
                list = [];
                linesBySupplier[key] = list;
            }

            var estimated = suggested.Value * product.AverageCost;
            list.Add(new PurchaseSuggestionLineDto(
                product.Id,
                product.Name,
                product.Sku,
                product.Barcode,
                onHand,
                product.ReorderLevel,
                product.TargetStock,
                suggested.Value,
                product.AverageCost,
                estimated));
        }

        var groups = new List<PurchaseSuggestionGroupDto>();
        foreach (var (supplierKey, lines) in linesBySupplier.OrderBy(k => k.Key))
        {
            Guid? supplierId = Guid.TryParse(supplierKey, out var sid) ? sid : null;
            Supplier? supplier = supplierId is Guid id && suppliers.TryGetValue(id, out var s) ? s : null;
            var name = supplier?.TradeName ?? supplier?.LegalName ?? "Sem fornecedor";
            var document = supplier?.Document;
            var ordered = lines.OrderBy(l => l.ProductName).ToList();
            groups.Add(new PurchaseSuggestionGroupDto(
                supplierId,
                name,
                document,
                ordered,
                ordered.Sum(l => l.EstimatedTotal)));
        }

        return groups;
    }

    private static async Task<decimal> ResolveOnHandAsync(
        IProductRepository productRepository,
        IProductLotRepository lotRepository,
        Guid productId,
        CancellationToken cancellationToken)
    {
        var lots = await lotRepository.ListByProductIdAsync(productId, cancellationToken);
        var totalFromLots = lots.Where(l => l.IsActive).Sum(l => l.Quantity);
        if (lots.Count > 0)
        {
            return totalFromLots;
        }

        var balance = await productRepository.GetBalanceAsync(productId, cancellationToken);
        return balance?.TotalQuantity ?? 0m;
    }
}
