using Core.Domain;
using Inventory.Domain.Entities;

namespace Inventory.Domain.Repositories;

/// <summary>
/// Persistence port for suppliers.
/// </summary>
public interface ISupplierRepository : IRepository<Supplier>
{
    /// <summary>Finds supplier by CNPJ digits.</summary>
    Task<Supplier?> GetByDocumentAsync(string document, CancellationToken cancellationToken = default);
}
