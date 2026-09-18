using Core.Domain;
using Inventory.Domain;
using Inventory.Domain.Enums;
using Inventory.Domain.ValueObjects;

namespace Inventory.Domain.Entities;

/// <summary>
/// Catalog product with fiscal basics and optional default supplier.
/// </summary>
public class Product : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public string Barcode { get; private set; } = string.Empty;
    public string UnitOfMeasure { get; private set; } = string.Empty;
    public decimal ReorderLevel { get; private set; }
    public ProductCategory Category { get; private set; }
    public Guid? SupplierId { get; private set; }
    public string Ncm { get; private set; } = string.Empty;
    public string? Cest { get; private set; }
    public int MerchandiseOrigin { get; private set; }
    public decimal AverageCost { get; private set; }
    public bool RequiresLot { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Product() { }

    private Product(
        Guid id,
        string name,
        string description,
        string sku,
        string barcode,
        string unitOfMeasure,
        decimal reorderLevel,
        ProductCategory category,
        Guid? supplierId,
        string ncm,
        string? cest,
        int merchandiseOrigin,
        bool requiresLot)
        : base(id)
    {
        Name = name;
        Description = description;
        Sku = sku;
        Barcode = barcode;
        UnitOfMeasure = unitOfMeasure;
        ReorderLevel = reorderLevel;
        Category = category;
        SupplierId = supplierId;
        Ncm = ncm;
        Cest = cest;
        MerchandiseOrigin = merchandiseOrigin;
        RequiresLot = requiresLot;
    }

    /// <summary>
    /// Creates a product with validated SKU, barcode and NCM.
    /// </summary>
    public static Result<Product> Create(
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
        bool? requiresLot = null,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Product>(ErrorCodes.Product.InvalidName);
        }

        var skuResult = ValueObjects.Sku.Create(sku);
        if (skuResult.IsFailure)
        {
            return Result.Failure<Product>(skuResult.Error);
        }

        var barcodeResult = ValueObjects.Barcode.Create(barcode);
        if (barcodeResult.IsFailure)
        {
            return Result.Failure<Product>(barcodeResult.Error);
        }

        var ncmResult = ValueObjects.Ncm.Create(ncm);
        if (ncmResult.IsFailure)
        {
            return Result.Failure<Product>(ncmResult.Error);
        }

        if (reorderLevel < 0)
        {
            return Result.Failure<Product>(ErrorCodes.Product.InvalidReorderLevel);
        }

        if (merchandiseOrigin is < 0 or > 8)
        {
            return Result.Failure<Product>(ErrorCodes.Product.InvalidMerchandiseOrigin);
        }

        var lotRequired = requiresLot ?? category is ProductCategory.Medication or ProductCategory.Vaccine;
        var productId = id ?? Guid.NewGuid();

        return Result.Success(new Product(
            productId,
            name.Trim(),
            description?.Trim() ?? string.Empty,
            skuResult.Value.Value,
            barcodeResult.Value.Value,
            string.IsNullOrWhiteSpace(unitOfMeasure) ? "UN" : unitOfMeasure.Trim(),
            reorderLevel,
            category,
            supplierId,
            ncmResult.Value.Value,
            string.IsNullOrWhiteSpace(cest) ? null : cest.Trim(),
            merchandiseOrigin,
            lotRequired));
    }

    /// <summary>
    /// Updates catalog fields (SKU/barcode uniqueness enforced in application layer).
    /// </summary>
    public Result UpdateDetails(
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
        bool requiresLot)
    {
        var created = Create(name, description, sku, barcode, unitOfMeasure, reorderLevel, category, ncm, cest, merchandiseOrigin, supplierId, requiresLot, Id);
        if (created.IsFailure)
        {
            return Result.Failure(created.Error);
        }

        var draft = created.Value;
        Name = draft.Name;
        Description = draft.Description;
        Sku = draft.Sku;
        Barcode = draft.Barcode;
        UnitOfMeasure = draft.UnitOfMeasure;
        ReorderLevel = draft.ReorderLevel;
        Category = draft.Category;
        SupplierId = supplierId;
        Ncm = draft.Ncm;
        Cest = draft.Cest;
        MerchandiseOrigin = draft.MerchandiseOrigin;
        RequiresLot = requiresLot;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Recalculates weighted average cost from lot snapshots.
    /// </summary>
    public void RecalculateAverageCost(decimal weightedAverage)
    {
        AverageCost = weightedAverage;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Soft-deactivates the product in catalog listings.
    /// </summary>
    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
