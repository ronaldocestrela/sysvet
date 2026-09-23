namespace Core.Application.Caching;

/// <summary>
/// Marks a MediatR query whose successful <see cref="Core.Domain.Result{T}"/> payload may be cached in <see cref="Microsoft.Extensions.Caching.Distributed.IDistributedCache"/>.
/// </summary>
public interface ICacheableQuery
{
    /// <summary>Distinct suffix for the cache entry (filters, period, etc.).</summary>
    string CacheKeySuffix { get; }

    /// <summary>Absolute TTL from the moment the value is stored.</summary>
    TimeSpan CacheDuration { get; }
}
