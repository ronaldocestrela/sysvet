using Inventory.Domain.Enums;

namespace Inventory.Application.Products.Dtos;

/// <summary>Product list row.</summary>
public sealed record ProductListItemDto(
    Guid Id,
    string Name,
    string Sku,
    string Barcode,
    ProductCategory Category,
    decimal TotalQuantity,
    decimal AverageCost,
    decimal ReorderLevel,
    decimal TargetStock,
    bool IsActive);

/// <summary>Product lot in detail views.</summary>
public sealed record ProductLotDto(
    Guid Id,
    string LotNumber,
    DateTimeOffset? ExpirationDate,
    decimal UnitCost,
    decimal Quantity,
    bool IsActive,
    bool IsFractional);

/// <summary>Full product with lots.</summary>
public sealed record ProductDetailDto(
    Guid Id,
    string Name,
    string Description,
    string Sku,
    string Barcode,
    string UnitOfMeasure,
    decimal ReorderLevel,
    decimal TargetStock,
    ProductCategory Category,
    Guid? SupplierId,
    string Ncm,
    string? Cest,
    int MerchandiseOrigin,
    decimal AverageCost,
    decimal TotalQuantity,
    bool RequiresLot,
    decimal UnitsPerPackage,
    decimal SealedQuantity,
    decimal FractionalQuantity,
    bool IsActive,
    IReadOnlyList<ProductLotDto> Lots);
