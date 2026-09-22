using System.Collections.Concurrent;
using Commerce.Application.Marketplace;

namespace Commerce.Infrastructure.Marketplace;

/// <summary>In-memory marketplace channel for tests and local PoC.</summary>
public sealed class FakeMarketplaceChannel : IMarketplaceChannel
{
    private readonly ConcurrentDictionary<string, MarketplaceRemoteOrder> _orders = new();
    private readonly ConcurrentQueue<MarketplaceListingPush> _pushes = new();

    /// <inheritdoc />
    public string ChannelName => "MercadoLivre";

    /// <summary>Recorded listing pushes for assertions.</summary>
    public IReadOnlyCollection<MarketplaceListingPush> Pushes => _pushes.ToArray();

    /// <summary>Seeds a remote order for webhook simulation.</summary>
    public void SeedOrder(MarketplaceRemoteOrder order) => _orders[order.ExternalOrderId] = order;

    /// <inheritdoc />
    public Task<MarketplacePushResult> PushListingAsync(MarketplaceListingPush push, CancellationToken cancellationToken = default)
    {
        _pushes.Enqueue(push);
        var listingId = push.ExternalListingId ?? $"ML-{push.Sku}";
        return Task.FromResult(new MarketplacePushResult(true, listingId, null));
    }

    /// <inheritdoc />
    public Task<MarketplaceRemoteOrder?> GetOrderAsync(string externalOrderId, CancellationToken cancellationToken = default)
    {
        _orders.TryGetValue(externalOrderId, out var order);
        return Task.FromResult(order);
    }
}
