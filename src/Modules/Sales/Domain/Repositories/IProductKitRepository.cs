using Sales.Domain.Entities;

namespace Sales.Domain.Repositories;

/// <summary>Persistence port for sellable product kits.</summary>
public interface IProductKitRepository
{
    Task<ProductKit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductKit>> ListAllAsync(CancellationToken cancellationToken = default);
    void Add(ProductKit kit);
    void Update(ProductKit kit);
}
