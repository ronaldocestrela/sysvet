using System.Text.Json;
using Core.Application.Caching;
using Core.Application.Entitlements;
using Core.Domain.Entitlements;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Distributed;
using Platform.Domain.Repositories;
using Platform.Domain.Services;

namespace Platform.Infrastructure.Entitlements;

/// <inheritdoc />
public sealed class TenantEntitlementReader : ITenantEntitlementReader
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ITenantSubscriptionRepository _subscriptionRepository;
    private readonly IFeatureFlagRepository _featureFlagRepository;
    private readonly IDistributedCache _cache;

    /// <summary>Creates the reader.</summary>
    public TenantEntitlementReader(
        ITenantSubscriptionRepository subscriptionRepository,
        IFeatureFlagRepository featureFlagRepository,
        IDistributedCache cache)
    {
        _subscriptionRepository = subscriptionRepository;
        _featureFlagRepository = featureFlagRepository;
        _cache = cache;
    }

    /// <inheritdoc />
    public async Task<IReadOnlySet<CommercialModule>> GetEnabledModulesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
        {
            return new HashSet<CommercialModule>();
        }

        var cacheKey = EntitlementCacheKeys.ForTenant(tenantId);
        var cachedBytes = await _cache.GetAsync(cacheKey, cancellationToken);
        if (cachedBytes is { Length: > 0 })
        {
            var modules = JsonSerializer.Deserialize<CommercialModule[]>(cachedBytes, JsonOptions);
            if (modules is not null)
            {
                return modules.ToHashSet();
            }
        }

        var effective = await LoadEffectiveAsync(tenantId, cancellationToken);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(effective.ToArray(), JsonOptions);
        await _cache.SetAsync(
            cacheKey,
            bytes,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheDurations.Analytics },
            cancellationToken);
        return effective;
    }

    /// <inheritdoc />
    public async Task<bool> IsModuleEnabledAsync(Guid tenantId, CommercialModule module, CancellationToken cancellationToken = default)
    {
        var set = await GetEnabledModulesAsync(tenantId, cancellationToken);
        return set.Contains(module);
    }

    /// <inheritdoc />
    public void Invalidate(Guid tenantId) => _cache.Remove(EntitlementCacheKeys.ForTenant(tenantId));

    private async Task<IReadOnlySet<CommercialModule>> LoadEffectiveAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByTenantIdAsync(tenantId, cancellationToken);
            // Tenants without a commercial subscription keep full module access until a plan is assigned.
            if (subscription?.Plan is null)
            {
                return AllModules();
            }

            var flags = await _featureFlagRepository.ListByTenantAsync(tenantId, cancellationToken);
            var planModules = subscription.Plan.GetModuleList();
            var addOnModules = subscription.AddOns.Select(a => a.AddOn!.Module);
            var flagTuples = flags.Select(f => (f.Module, f.State));
            return EntitlementCalculator.ComputeEffective(subscription.Status, planModules, addOnModules, flagTuples);
        }
        catch (SqliteException)
        {
            // Shared SQLite files used by integration tests are wiped with EnsureDeleted on another module.
            // Until Platform migrations run again, do not block clinic APIs.
            return AllModules();
        }
    }

    private static HashSet<CommercialModule> AllModules() => Enum.GetValues<CommercialModule>().ToHashSet();
}
