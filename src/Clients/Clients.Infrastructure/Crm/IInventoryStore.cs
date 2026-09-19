using Core.Domain;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Services;

namespace Clients.Infrastructure.Crm;

/// <summary>
/// Offline-first inventory catalog store (products, lots, suppliers).
/// </summary>
public interface IInventoryStore
{
    Task<Result<IReadOnlyList<InventoryProductListItem>>> ListProductsAsync(bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<Result<InventoryProductDetail>> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<Result<Guid>> RegisterProductAsync(
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
        decimal? unitsPerPackage = null,
        CancellationToken cancellationToken = default);
    Task<Result<Guid>> RegisterProductLotAsync(
        Guid productId,
        string lotNumber,
        DateTimeOffset? expirationDate,
        decimal unitCost,
        decimal initialQuantity,
        CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<InventorySupplierItem>>> ListSuppliersAsync(bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<Result<Guid>> RegisterSupplierAsync(
        string legalName,
        string tradeName,
        string document,
        string? contactEmail,
        string? contactPhone,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> RegisterStockMovementAsync(
        Guid productId,
        MovementType type,
        decimal quantity,
        string reason,
        Guid? productLotId = null,
        AdjustmentDirection? adjustmentDirection = null,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> TransferStockAsync(
        Guid productId,
        Guid sourceLotId,
        Guid destinationLotId,
        decimal quantity,
        string reason,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> RegisterStockLossAsync(
        Guid productId,
        Guid? productLotId,
        decimal quantity,
        string lossReasonCode,
        string? notes = null,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> FractionatePackageAsync(
        Guid productId,
        Guid sealedLotId,
        decimal packagesToOpen,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> RegisterSupplierReturnAsync(
        Guid productId,
        Guid? productLotId,
        decimal quantity,
        Guid? supplierId,
        string? notes = null,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<InventoryStockMovementItem>>> ListStockMovementsAsync(
        Guid? productId = null,
        int take = 50,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<InventoryKardexLine>>> GetProductKardexAsync(
        Guid productId,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<InventoryStockAlertItem>>> GetStockAlertsAsync(
        StockAlertKind? kind = null,
        int horizonDays = 30,
        CancellationToken cancellationToken = default);
}

public sealed class InventoryProductListItem
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public string Barcode { get; init; } = string.Empty;
    public ProductCategory Category { get; init; }
    public decimal TotalQuantity { get; init; }
    public decimal AverageCost { get; init; }
    public decimal ReorderLevel { get; init; }
    public bool IsActive { get; init; }
}

public sealed class InventoryProductLotItem
{
    public Guid Id { get; init; }
    public string LotNumber { get; init; } = string.Empty;
    public DateTimeOffset? ExpirationDate { get; init; }
    public decimal UnitCost { get; init; }
    public decimal Quantity { get; init; }
    public bool IsActive { get; init; }
    public bool IsFractional { get; init; }
}

public sealed class InventoryProductDetail
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public string Barcode { get; init; } = string.Empty;
    public string UnitOfMeasure { get; init; } = string.Empty;
    public decimal ReorderLevel { get; init; }
    public ProductCategory Category { get; init; }
    public Guid? SupplierId { get; init; }
    public string Ncm { get; init; } = string.Empty;
    public string? Cest { get; init; }
    public int MerchandiseOrigin { get; init; }
    public decimal AverageCost { get; init; }
    public decimal TotalQuantity { get; init; }
    public bool RequiresLot { get; init; }
    public decimal UnitsPerPackage { get; init; } = 1m;
    public decimal SealedQuantity { get; init; }
    public decimal FractionalQuantity { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyList<InventoryProductLotItem> Lots { get; init; } = Array.Empty<InventoryProductLotItem>();
}

public sealed class InventoryStockMovementItem
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public MovementType Type { get; init; }
    public decimal Quantity { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? LotNumber { get; init; }
    public DateTimeOffset Date { get; init; }
}

public sealed class InventoryKardexLine
{
    public DateTimeOffset Date { get; init; }
    public MovementType Type { get; init; }
    public decimal Quantity { get; init; }
    public decimal RunningBalance { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? LotNumber { get; init; }
}

public sealed class InventoryStockAlertItem
{
    public StockAlertKind Kind { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public string? LotNumber { get; init; }
    public decimal? TotalQuantity { get; init; }
    public decimal? ReorderLevel { get; init; }
    public DateTimeOffset? ExpirationDate { get; init; }
}

public sealed class InventorySupplierItem
{
    public Guid Id { get; init; }
    public string LegalName { get; init; } = string.Empty;
    public string TradeName { get; init; } = string.Empty;
    public string Document { get; init; } = string.Empty;
    public string? ContactEmail { get; init; }
    public string? ContactPhone { get; init; }
    public bool IsActive { get; init; }
}
