using Commerce.Domain.Entities;
using Commerce.Domain.Enums;

namespace Commerce.Domain.Repositories;

/// <summary>Outbox persistence for marketplace sync jobs.</summary>
public interface IMarketplaceSyncJobRepository
{
    Task<IReadOnlyList<MarketplaceSyncJob>> ListDueAsync(int batchSize, DateTimeOffset now, CancellationToken cancellationToken = default);
    Task<MarketplaceSyncJob?> GetByIdempotencyKeyAsync(string key, CancellationToken cancellationToken = default);
    void Add(MarketplaceSyncJob job);
    void Update(MarketplaceSyncJob job);
}
