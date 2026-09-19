using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Domain;
using Inventory.Domain.Entities;

namespace Inventory.Domain.Repositories;

public interface IStockMovementRepository : IRepository<StockMovement>
{
    /// <summary>Lists movements for a product ordered by date ascending.</summary>
    Task<IReadOnlyList<StockMovement>> ListByProductAsync(Guid productId, CancellationToken cancellationToken = default);

    /// <summary>Lists recent movements optionally filtered by product.</summary>
    Task<IReadOnlyList<StockMovement>> ListRecentAsync(Guid? productId, string? reason, int skip, int take, CancellationToken cancellationToken = default);
}
