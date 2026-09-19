using Core.Domain;
using Inventory.Domain;

namespace Inventory.Domain.Entities;

/// <summary>
/// Maps a supplier catalog code (cProd) to an internal product for future NF-e imports.
/// </summary>
public class SupplierProductMapping : Entity
{
    public Guid SupplierId { get; private set; }
    public string SupplierProductCode { get; private set; } = string.Empty;
    public Guid ProductId { get; private set; }

    private SupplierProductMapping() { }

    /// <summary>
    /// Creates a mapping row for the tenant supplier product code.
    /// </summary>
    public static Result<SupplierProductMapping> Create(Guid supplierId, string supplierProductCode, Guid productId, Guid? id = null)
    {
        if (supplierId == Guid.Empty)
        {
            return Result.Failure<SupplierProductMapping>(ErrorCodes.Supplier.NotFound);
        }

        if (string.IsNullOrWhiteSpace(supplierProductCode))
        {
            return Result.Failure<SupplierProductMapping>(ErrorCodes.PurchaseImport.InvalidSupplierProductCode);
        }

        if (productId == Guid.Empty)
        {
            return Result.Failure<SupplierProductMapping>(ErrorCodes.Product.NotFound);
        }

        return Result.Success(new SupplierProductMapping
        {
            Id = id ?? Guid.NewGuid(),
            SupplierId = supplierId,
            SupplierProductCode = supplierProductCode.Trim(),
            ProductId = productId
        });
    }
}
