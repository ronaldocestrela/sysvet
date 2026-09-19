using Core.Domain;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;

namespace Inventory.Domain.Repositories;

/// <summary>
/// Persistence port for physical inventory count sessions.
/// </summary>
public interface IInventoryCountRepository : IRepository<InventoryCount>
{
    /// <summary>Loads session with lines for count workflow.</summary>
    Task<InventoryCount?> GetByIdWithLinesAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Returns the open blind-count session for the tenant, if any.</summary>
    Task<InventoryCount?> GetInProgressAsync(CancellationToken cancellationToken = default);

    /// <summary>Lists recent sessions ordered by creation descending.</summary>
    Task<IReadOnlyList<InventoryCount>> ListRecentAsync(int take, CancellationToken cancellationToken = default);

    /// <summary>Stages a new line when EF did not pick up the aggregate navigation change.</summary>
    void StageLine(InventoryCountLine line);
}
