using Core.Domain;
using Inventory.Domain.Entities;

namespace Inventory.Domain.Repositories;

/// <summary>
/// Persistence port for product lots.
/// </summary>
public interface IProductLotRepository : IRepository<ProductLot>
{
    /// <summary>Lists lots for a product.</summary>
    Task<IReadOnlyList<ProductLot>> ListByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);

    /// <summary>Finds lot by product and lot number.</summary>
    Task<ProductLot?> GetByProductAndLotNumberAsync(Guid productId, string lotNumber, CancellationToken cancellationToken = default);
}
