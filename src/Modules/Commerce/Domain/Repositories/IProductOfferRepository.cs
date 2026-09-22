using Commerce.Domain.Entities;

namespace Commerce.Domain.Repositories;

/// <summary>Persistence port for storefront product offers.</summary>
public interface IProductOfferRepository
{
    Task<ProductOffer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductOffer?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductOffer>> ListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductOffer>> ListPublishedForStoreAsync(CancellationToken cancellationToken = default);
    void Add(ProductOffer offer);
    void Update(ProductOffer offer);
}
