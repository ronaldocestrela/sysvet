using Microsoft.EntityFrameworkCore;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Persistence.Repositories;

/// <inheritdoc />
public sealed class TenantSubscriptionRepository : ITenantSubscriptionRepository
{
    private readonly PlatformDbContext _context;

    /// <summary>Creates the repository.</summary>
    public TenantSubscriptionRepository(PlatformDbContext context) => _context = context;

    /// <inheritdoc />
    public Task<TenantSubscription?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _context.TenantSubscriptions
            .Include(s => s.Plan)!.ThenInclude(p => p!.IncludedModules)
            .Include(s => s.AddOns).ThenInclude(a => a.AddOn)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<TenantSubscription>> ListDueTrialsAsync(DateTimeOffset asOfUtc, CancellationToken cancellationToken = default) =>
        _context.TenantSubscriptions
            .Where(s => s.Status == SubscriptionStatus.Trial && s.TrialEndsAt != null && s.TrialEndsAt <= asOfUtc)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<TenantSubscription>)t.Result, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<TenantSubscription>> ListDueForBillingAsync(DateTimeOffset asOfUtc, CancellationToken cancellationToken = default) =>
        _context.TenantSubscriptions
            .Include(s => s.Plan)
            .Include(s => s.AddOns).ThenInclude(a => a.AddOn)
            .Where(s => s.Status == SubscriptionStatus.Active
                        && s.BillingStanding != BillingStanding.Canceled
                        && s.PeriodEnd <= asOfUtc)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<TenantSubscription>)t.Result, cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(TenantSubscription subscription, CancellationToken cancellationToken = default) =>
        await _context.TenantSubscriptions.AddAsync(subscription, cancellationToken);
}
