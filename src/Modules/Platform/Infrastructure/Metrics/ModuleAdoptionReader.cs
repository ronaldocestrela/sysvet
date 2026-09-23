using Core.Domain.Entitlements;
using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions;
using Platform.Domain.Entities;
using Platform.Domain.Services;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Metrics;

/// <inheritdoc />
public sealed class ModuleAdoptionReader : IModuleAdoptionReader
{
    private readonly PlatformDbContext _context;

    /// <summary>Creates the reader.</summary>
    public ModuleAdoptionReader(PlatformDbContext context) => _context = context;

    /// <inheritdoc />
    public async Task<ModuleAdoptionSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var tenants = await _context.Tenants
            .AsNoTracking()
            .Where(t => t.Status != TenantStatus.Cancelled && t.Status != TenantStatus.Deleted)
            .OrderBy(t => t.DisplayName)
            .ToListAsync(cancellationToken);

        var subscriptions = await _context.TenantSubscriptions
            .AsNoTracking()
            .Include(s => s.Plan!)
            .ThenInclude(p => p!.IncludedModules)
            .Include(s => s.AddOns)
            .ThenInclude(a => a.AddOn)
            .ToListAsync(cancellationToken);

        var subscriptionByTenant = subscriptions.ToDictionary(s => s.TenantId);

        var flags = await _context.FeatureFlags.AsNoTracking().ToListAsync(cancellationToken);
        var flagsByTenant = flags.GroupBy(f => f.TenantId).ToDictionary(g => g.Key, g => g.ToList());

        var inputs = tenants.Select(tenant =>
        {
            var effective = ComputeEffective(tenant.Id, subscriptionByTenant, flagsByTenant);
            return new TenantModuleAdoptionInput(tenant.Id, tenant.DisplayName, effective);
        }).ToList();

        return ModuleAdoptionCalculator.Calculate(inputs);
    }

    private static IReadOnlySet<CommercialModule> ComputeEffective(
        Guid tenantId,
        IReadOnlyDictionary<Guid, TenantSubscription> subscriptions,
        IReadOnlyDictionary<Guid, List<FeatureFlag>> flagsByTenant)
    {
        if (!subscriptions.TryGetValue(tenantId, out var subscription) || subscription.Plan is null)
        {
            return AllModules();
        }

        flagsByTenant.TryGetValue(tenantId, out var tenantFlags);
        tenantFlags ??= [];

        var planModules = subscription.Plan.GetModuleList();
        var addOnModules = subscription.AddOns.Select(a => a.AddOn!.Module);
        var flagTuples = tenantFlags.Select(f => (f.Module, f.State));
        return EntitlementCalculator.ComputeEffective(subscription.Status, planModules, addOnModules, flagTuples);
    }

    private static HashSet<CommercialModule> AllModules() => Enum.GetValues<CommercialModule>().ToHashSet();
}
