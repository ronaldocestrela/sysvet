using Fiscal.Domain.Entities;
using Fiscal.Domain.Enums;

namespace Fiscal.Domain.Repositories;

/// <summary>Fiscal document persistence.</summary>
public interface IFiscalDocumentRepository
{
    /// <summary>Gets document by id.</summary>
    Task<FiscalDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Finds authorized document for order and type.</summary>
    Task<FiscalDocument?> GetAuthorizedByOrderAsync(
        Guid orderId,
        FiscalDocumentType type,
        CancellationToken cancellationToken = default);

    /// <summary>Lists documents with optional filters.</summary>
    Task<IReadOnlyList<FiscalDocument>> ListAsync(
        Guid? orderId,
        FiscalDocumentStatus? status,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a new document.</summary>
    void Add(FiscalDocument document);

    /// <summary>Updates document state.</summary>
    void Update(FiscalDocument document);
}
