using Finance.Domain.Entities;

namespace Finance.Domain.Repositories;

/// <summary>
/// Persistence for card reconciliation batches.
/// </summary>
public interface ICardReconciliationRepository
{
    Task<CardReconciliationBatch?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<CardReconciliationBatch>> ListAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Guid>> ListMatchedAllocationIdsAsync(CancellationToken cancellationToken);

    void Add(CardReconciliationBatch batch);
}
