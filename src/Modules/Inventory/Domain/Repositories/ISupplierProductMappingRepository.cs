using Core.Domain;
using Inventory.Domain.Entities;

namespace Inventory.Domain.Repositories;

/// <summary>
/// Persistence port for supplier product code mappings.
/// </summary>
public interface ISupplierProductMappingRepository : IRepository<SupplierProductMapping>
{
    /// <summary>Finds mapping for supplier and NF cProd code.</summary>
    Task<SupplierProductMapping?> GetBySupplierAndCodeAsync(
        Guid supplierId,
        string supplierProductCode,
        CancellationToken cancellationToken = default);
}
