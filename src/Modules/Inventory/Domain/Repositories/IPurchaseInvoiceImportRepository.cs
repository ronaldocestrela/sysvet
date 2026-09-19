using Core.Domain;
using Inventory.Domain.Entities;

namespace Inventory.Domain.Repositories;

/// <summary>
/// Persistence port for purchase NF-e imports.
/// </summary>
public interface IPurchaseInvoiceImportRepository : IRepository<PurchaseInvoiceImport>
{
    /// <summary>Loads import with lines for preview and confirm.</summary>
    Task<PurchaseInvoiceImport?> GetByIdWithLinesAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Finds import by NF-e access key.</summary>
    Task<PurchaseInvoiceImport?> GetByAccessKeyAsync(string accessKey, CancellationToken cancellationToken = default);

    /// <summary>Lists recent imports ordered by issue date descending.</summary>
    Task<IReadOnlyList<PurchaseInvoiceImport>> ListRecentAsync(int take, CancellationToken cancellationToken = default);
}
