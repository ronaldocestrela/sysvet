using Clients.Infrastructure.Sync;
using Core.Domain;
using Inventory.Domain.Entities;
using Inventory.Domain.Services;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Clients.Infrastructure.Crm;

/// <summary>SQLite-backed inventory catalog with sync outbox.</summary>
public sealed partial class OfflineInventoryStore : IInventoryStore
{
    private readonly OfflineDbContext _dbContext;

    public OfflineInventoryStore(OfflineDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<IReadOnlyList<InventoryProductListItem>>> ListProductsAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Products.AsNoTracking();
        if (activeOnly)
        {
            query = query.Where(p => p.IsActive);
        }

        var products = await query.OrderBy(p => p.Name).ToListAsync(cancellationToken);
        var items = new List<InventoryProductListItem>();
        foreach (var product in products)
        {
            var balance = await _dbContext.ProductBalances.AsNoTracking().FirstOrDefaultAsync(b => b.ProductId == product.Id, cancellationToken);
            items.Add(MapList(product, balance?.TotalQuantity ?? 0));
        }

        return Result.Success<IReadOnlyList<InventoryProductListItem>>(items);
    }

    public async Task<Result<InventoryProductDetail>> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product is null)
        {
            return Result.Failure<InventoryProductDetail>(Inventory.Domain.ErrorCodes.Product.NotFound);
        }

        var balance = await _dbContext.ProductBalances.AsNoTracking().FirstOrDefaultAsync(b => b.ProductId == productId, cancellationToken);
        var lots = await _dbContext.ProductLots.AsNoTracking().Where(l => l.ProductId == productId).OrderBy(l => l.LotNumber).ToListAsync(cancellationToken);
        return Result.Success(MapDetail(product, balance?.TotalQuantity ?? 0, lots));
    }

    public async Task<Result<Guid>> RegisterProductAsync(
        string name,
        string description,
        string sku,
        string barcode,
        string unitOfMeasure,
        decimal reorderLevel,
        ProductCategory category,
        string ncm,
        string? cest,
        int merchandiseOrigin,
        Guid? supplierId,
        bool? requiresLot,
        CancellationToken cancellationToken = default)
    {
        if (await _dbContext.Products.AnyAsync(p => p.Sku == sku.Trim().ToUpperInvariant(), cancellationToken))
        {
            return Result.Failure<Guid>(Inventory.Domain.ErrorCodes.Product.SkuConflict);
        }

        var id = Guid.NewGuid();
        var created = Product.Create(name, description, sku, barcode, unitOfMeasure, reorderLevel, category, ncm, cest, merchandiseOrigin, supplierId, requiresLot, id);
        if (created.IsFailure)
        {
            return Result.Failure<Guid>(created.Error);
        }

        _dbContext.Products.Add(created.Value);
        await _dbContext.ProductBalances.AddAsync(new ProductBalance(id, 0m), cancellationToken);
        EnqueueOutbox("RegisterProductCommand",
            OutboxPayloadFactory.RegisterProduct(id, name, description, sku, barcode, unitOfMeasure, reorderLevel, category, ncm, cest, merchandiseOrigin, supplierId, requiresLot, id));
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(id);
    }

    public async Task<Result<Guid>> RegisterProductLotAsync(
        Guid productId,
        string lotNumber,
        DateTimeOffset? expirationDate,
        decimal unitCost,
        decimal initialQuantity,
        CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product is null)
        {
            return Result.Failure<Guid>(Inventory.Domain.ErrorCodes.Product.NotFound);
        }

        var lotId = Guid.NewGuid();
        var lotResult = ProductLot.Create(productId, lotNumber, expirationDate, unitCost, initialQuantity, lotId);
        if (lotResult.IsFailure)
        {
            return Result.Failure<Guid>(lotResult.Error);
        }

        _dbContext.ProductLots.Add(lotResult.Value);
        var balance = await _dbContext.ProductBalances.FirstOrDefaultAsync(b => b.ProductId == productId, cancellationToken);
        if (balance is null)
        {
            balance = new ProductBalance(productId, 0m);
            await _dbContext.ProductBalances.AddAsync(balance, cancellationToken);
        }

        var lots = await _dbContext.ProductLots.Where(l => l.ProductId == productId).ToListAsync(cancellationToken);
        lots.Add(lotResult.Value);
        balance.SyncFromLots(InventoryCostCalculator.TotalQuantityFromLots(lots.Select(l => (l.Quantity, l.IsActive))));
        product.RecalculateAverageCost(InventoryCostCalculator.WeightedAverageCost(lots.Where(l => l.IsActive).Select(l => (l.Quantity, l.UnitCost))));
        _dbContext.Products.Update(product);
        _dbContext.ProductBalances.Update(balance);

        EnqueueOutbox("RegisterProductLotCommand",
            OutboxPayloadFactory.RegisterProductLot(productId, lotNumber, expirationDate, unitCost, initialQuantity, lotId, lotId));
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(lotId);
    }

    public async Task<Result<IReadOnlyList<InventorySupplierItem>>> ListSuppliersAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Suppliers.AsNoTracking();
        if (activeOnly)
        {
            query = query.Where(s => s.IsActive);
        }

        var list = await query.OrderBy(s => s.TradeName).Select(s => new InventorySupplierItem
        {
            Id = s.Id,
            LegalName = s.LegalName,
            TradeName = s.TradeName,
            Document = s.Document,
            ContactEmail = s.ContactEmail,
            ContactPhone = s.ContactPhone,
            IsActive = s.IsActive
        }).ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<InventorySupplierItem>>(list);
    }

    public async Task<Result<Guid>> RegisterSupplierAsync(
        string legalName,
        string tradeName,
        string document,
        string? contactEmail,
        string? contactPhone,
        CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid();
        var created = Supplier.Create(legalName, tradeName, document, contactEmail, contactPhone, id);
        if (created.IsFailure)
        {
            return Result.Failure<Guid>(created.Error);
        }

        _dbContext.Suppliers.Add(created.Value);
        EnqueueOutbox("RegisterSupplierCommand",
            OutboxPayloadFactory.RegisterSupplier(id, legalName, tradeName, document, contactEmail, contactPhone, id));
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(id);
    }

    private void EnqueueOutbox(string type, string payload)
    {
        _dbContext.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = type,
            Payload = payload
        });
    }

    private static InventoryProductListItem MapList(Product product, decimal total) =>
        new()
        {
            Id = product.Id,
            Name = product.Name,
            Sku = product.Sku,
            Barcode = product.Barcode,
            Category = product.Category,
            TotalQuantity = total,
            AverageCost = product.AverageCost,
            ReorderLevel = product.ReorderLevel,
            IsActive = product.IsActive
        };

    private static InventoryProductDetail MapDetail(Product product, decimal total, IReadOnlyList<ProductLot> lots) =>
        new()
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Sku = product.Sku,
            Barcode = product.Barcode,
            UnitOfMeasure = product.UnitOfMeasure,
            ReorderLevel = product.ReorderLevel,
            Category = product.Category,
            SupplierId = product.SupplierId,
            Ncm = product.Ncm,
            Cest = product.Cest,
            MerchandiseOrigin = product.MerchandiseOrigin,
            AverageCost = product.AverageCost,
            TotalQuantity = total,
            RequiresLot = product.RequiresLot,
            IsActive = product.IsActive,
            Lots = lots.Select(l => new InventoryProductLotItem
            {
                Id = l.Id,
                LotNumber = l.LotNumber,
                ExpirationDate = l.ExpirationDate,
                UnitCost = l.UnitCost,
                Quantity = l.Quantity,
                IsActive = l.IsActive
            }).ToList()
        };
}
