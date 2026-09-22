using Commerce.Domain.Entities;
using Commerce.Domain.Enums;
using Commerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Infrastructure.Persistence.Repositories;

/// <summary>EF product offer repository.</summary>
public sealed class ProductOfferRepository : IProductOfferRepository
{
    private readonly CommerceDbContext _db;

    public ProductOfferRepository(CommerceDbContext db) => _db = db;

    public Task<ProductOffer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.ProductOffers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<ProductOffer?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default) =>
        _db.ProductOffers.FirstOrDefaultAsync(x => x.ProductId == productId, cancellationToken);

    public async Task<IReadOnlyList<ProductOffer>> ListAsync(CancellationToken cancellationToken = default) =>
        await _db.ProductOffers.OrderBy(x => x.ProductName).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ProductOffer>> ListPublishedForStoreAsync(CancellationToken cancellationToken = default) =>
        await _db.ProductOffers
            .Where(x => x.IsPublished && x.StoreEnabled)
            .OrderBy(x => x.ProductName)
            .ToListAsync(cancellationToken);

    public void Add(ProductOffer offer) => _db.ProductOffers.Add(offer);

    public void Update(ProductOffer offer) => _db.ProductOffers.Update(offer);
}

/// <summary>EF online order repository.</summary>
public sealed class OnlineOrderRepository : IOnlineOrderRepository
{
    private readonly CommerceDbContext _db;

    public OnlineOrderRepository(CommerceDbContext db) => _db = db;

    public async Task<OnlineOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _db.OnlineOrders.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (order is null)
        {
            return null;
        }

        await _db.Entry(order).Collection<OnlineOrderLine>("_lines").LoadAsync(cancellationToken);
        return order;
    }

    public async Task<OnlineOrder?> GetByExternalOrderIdAsync(string externalOrderId, CancellationToken cancellationToken = default)
    {
        var order = await _db.OnlineOrders.FirstOrDefaultAsync(x => x.ExternalOrderId == externalOrderId, cancellationToken);
        if (order is null)
        {
            return null;
        }

        await _db.Entry(order).Collection<OnlineOrderLine>("_lines").LoadAsync(cancellationToken);
        return order;
    }

    public async Task<IReadOnlyList<OnlineOrder>> ListAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _db.OnlineOrders.OrderByDescending(x => x.UpdatedAt).ToListAsync(cancellationToken);
        foreach (var order in orders)
        {
            await _db.Entry(order).Collection<OnlineOrderLine>("_lines").LoadAsync(cancellationToken);
        }

        return orders;
    }

    public void Add(OnlineOrder order) => _db.OnlineOrders.Add(order);

    public void Update(OnlineOrder order) => _db.OnlineOrders.Update(order);
}

/// <summary>EF marketplace sync job repository.</summary>
public sealed class MarketplaceSyncJobRepository : IMarketplaceSyncJobRepository
{
    private readonly CommerceDbContext _db;

    public MarketplaceSyncJobRepository(CommerceDbContext db) => _db = db;

    public async Task<IReadOnlyList<MarketplaceSyncJob>> ListDueAsync(int batchSize, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        var jobs = await _db.MarketplaceSyncJobs.ToListAsync(cancellationToken);
        return jobs
            .Where(j => j.Status == MarketplaceSyncJobStatus.Pending && j.NextAttemptAt <= now)
            .OrderBy(j => j.NextAttemptAt)
            .Take(batchSize)
            .ToList();
    }

    public Task<MarketplaceSyncJob?> GetByIdempotencyKeyAsync(string key, CancellationToken cancellationToken = default) =>
        _db.MarketplaceSyncJobs.FirstOrDefaultAsync(j => j.IdempotencyKey == key, cancellationToken);

    public void Add(MarketplaceSyncJob job) => _db.MarketplaceSyncJobs.Add(job);

    public void Update(MarketplaceSyncJob job) => _db.MarketplaceSyncJobs.Update(job);
}

/// <summary>EF Mercado Livre settings repository.</summary>
public sealed class MercadoLivreSettingsRepository : IMercadoLivreSettingsRepository
{
    private readonly CommerceDbContext _db;

    public MercadoLivreSettingsRepository(CommerceDbContext db) => _db = db;

    public Task<MercadoLivreSettings?> GetAsync(CancellationToken cancellationToken = default) =>
        _db.MercadoLivreSettings.FirstOrDefaultAsync(x => x.Key == MercadoLivreSettings.SingletonKey, cancellationToken);

    public void Add(MercadoLivreSettings settings) => _db.MercadoLivreSettings.Add(settings);

    public void Update(MercadoLivreSettings settings) => _db.MercadoLivreSettings.Update(settings);
}

/// <summary>EF global seller index repository.</summary>
public sealed class MarketplaceSellerIndexRepository : IMarketplaceSellerIndexRepository
{
    private readonly CommerceDbContext _db;

    public MarketplaceSellerIndexRepository(CommerceDbContext db) => _db = db;

    public Task<MarketplaceSellerIndex?> GetByMercadoLivreUserIdAsync(long userId, CancellationToken cancellationToken = default) =>
        _db.MarketplaceSellerIndexes.FirstOrDefaultAsync(x => x.MercadoLivreUserId == userId, cancellationToken);

    public async Task UpsertAsync(long mercadoLivreUserId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var existing = await GetByMercadoLivreUserIdAsync(mercadoLivreUserId, cancellationToken);
        if (existing is null)
        {
            _db.MarketplaceSellerIndexes.Add(MarketplaceSellerIndex.Create(mercadoLivreUserId, tenantId));
            return;
        }

        existing.SetTenantId(tenantId);
        _db.MarketplaceSellerIndexes.Update(existing);
    }
}
