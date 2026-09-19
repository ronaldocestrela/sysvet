using Core.Application.Sync;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Sync;

/// <summary>
/// Exposes inventory catalog rows to the Core sync pull feed.
/// </summary>
public sealed class InventorySyncChangeFeedContributor : ISyncChangeFeedContributor
{
    private readonly InventoryDbContext _dbContext;

    /// <summary>Creates the contributor.</summary>
    public InventorySyncChangeFeedContributor(InventoryDbContext dbContext) => _dbContext = dbContext;

    /// <inheritdoc />
    public async Task<SyncContributorChanges> ReadChangesAsync(DateTimeOffset since, int take, CancellationToken cancellationToken)
    {
        var maxUpdated = since;
        var hasMore = false;

        var products = await ReadPageAsync(
            _dbContext.Products.AsNoTracking(),
            since,
            take,
            p => p.UpdatedAt,
            cancellationToken);
        hasMore |= products.HasMore;
        maxUpdated = Max(maxUpdated, products.Items.Select(p => p.UpdatedAt));

        var lots = await ReadPageAsync(
            _dbContext.ProductLots.AsNoTracking(),
            since,
            take,
            l => l.UpdatedAt,
            cancellationToken);
        hasMore |= lots.HasMore;
        maxUpdated = Max(maxUpdated, lots.Items.Select(l => l.UpdatedAt));

        var suppliers = await ReadPageAsync(
            _dbContext.Suppliers.AsNoTracking(),
            since,
            take,
            s => s.UpdatedAt,
            cancellationToken);
        hasMore |= suppliers.HasMore;
        maxUpdated = Max(maxUpdated, suppliers.Items.Select(s => s.UpdatedAt));

        var movements = await ReadPageAsync(
            _dbContext.StockMovements.AsNoTracking(),
            since,
            take,
            m => m.UpdatedAt,
            cancellationToken);
        hasMore |= movements.HasMore;
        maxUpdated = Max(maxUpdated, movements.Items.Select(m => m.UpdatedAt));

        return new SyncContributorChanges
        {
            InventoryProducts = products.Items.Select(MapProduct).ToList(),
            InventoryProductLots = lots.Items.Select(MapLot).ToList(),
            InventorySuppliers = suppliers.Items.Select(MapSupplier).ToList(),
            InventoryStockMovements = movements.Items.Select(MapMovement).ToList(),
            MaxUpdatedAt = maxUpdated,
            HasMore = hasMore
        };
    }

    private static DateTimeOffset Max(DateTimeOffset current, IEnumerable<DateTimeOffset> values)
    {
        foreach (var value in values)
        {
            if (value > current)
            {
                current = value;
            }
        }

        return current;
    }

    private static async Task<(List<T> Items, bool HasMore)> ReadPageAsync<T>(
        IQueryable<T> query,
        DateTimeOffset since,
        int take,
        Func<T, DateTimeOffset> updatedAt,
        CancellationToken cancellationToken)
    {
        var all = await query.ToListAsync(cancellationToken);
        var candidates = all.Where(x => updatedAt(x) > since).OrderBy(updatedAt).Take(take + 1).ToList();
        var hasMore = candidates.Count > take;
        if (hasMore)
        {
            candidates = candidates.Take(take).ToList();
        }

        return (candidates, hasMore);
    }

    private static SyncInventoryProductDto MapProduct(Product p) =>
        new()
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Sku = p.Sku,
            Barcode = p.Barcode,
            UnitOfMeasure = p.UnitOfMeasure,
            ReorderLevel = p.ReorderLevel,
            Category = p.Category.ToString(),
            SupplierId = p.SupplierId,
            Ncm = p.Ncm,
            Cest = p.Cest,
            MerchandiseOrigin = p.MerchandiseOrigin,
            AverageCost = p.AverageCost,
            RequiresLot = p.RequiresLot,
            UnitsPerPackage = p.UnitsPerPackage,
            IsActive = p.IsActive,
            UpdatedAt = p.UpdatedAt,
            RowVersion = Convert.ToBase64String(p.RowVersion ?? Array.Empty<byte>())
        };

    private static SyncInventoryProductLotDto MapLot(ProductLot l) =>
        new()
        {
            Id = l.Id,
            ProductId = l.ProductId,
            LotNumber = l.LotNumber,
            ExpirationDate = l.ExpirationDate,
            UnitCost = l.UnitCost,
            Quantity = l.Quantity,
            IsFractional = l.IsFractional,
            IsActive = l.IsActive,
            UpdatedAt = l.UpdatedAt,
            RowVersion = Convert.ToBase64String(l.RowVersion ?? Array.Empty<byte>())
        };

    private static SyncInventoryStockMovementDto MapMovement(StockMovement m) =>
        new()
        {
            Id = m.Id,
            ProductId = m.ProductId,
            ProductLotId = m.ProductLotId,
            Type = m.Type.ToString(),
            AdjustmentDirection = m.AdjustmentDirection?.ToString(),
            Quantity = m.Quantity,
            BatchNumber = m.BatchNumber,
            ExpirationDate = m.ExpirationDate,
            Reason = m.Reason,
            Notes = m.Notes,
            SupplierId = m.SupplierId,
            Date = m.Date,
            CorrelationId = m.CorrelationId,
            UpdatedAt = m.UpdatedAt,
            RowVersion = Convert.ToBase64String(m.RowVersion ?? Array.Empty<byte>())
        };

    private static SyncInventorySupplierDto MapSupplier(Supplier s) =>
        new()
        {
            Id = s.Id,
            LegalName = s.LegalName,
            TradeName = s.TradeName,
            Document = s.Document,
            ContactEmail = s.ContactEmail,
            ContactPhone = s.ContactPhone,
            IsActive = s.IsActive,
            UpdatedAt = s.UpdatedAt,
            RowVersion = Convert.ToBase64String(s.RowVersion ?? Array.Empty<byte>())
        };
}
