using Commerce.Domain.Entities;

namespace Commerce.Domain.Repositories;

/// <summary>Global Mercado Livre seller to tenant index.</summary>
public interface IMarketplaceSellerIndexRepository
{
    Task<MarketplaceSellerIndex?> GetByMercadoLivreUserIdAsync(long userId, CancellationToken cancellationToken = default);
    Task UpsertAsync(long mercadoLivreUserId, Guid tenantId, CancellationToken cancellationToken = default);
}
