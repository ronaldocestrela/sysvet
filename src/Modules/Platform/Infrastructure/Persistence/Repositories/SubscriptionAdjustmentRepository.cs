using Microsoft.EntityFrameworkCore;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Persistence.Repositories;

/// <inheritdoc />
public sealed class SubscriptionAdjustmentRepository : ISubscriptionAdjustmentRepository
{
    private readonly PlatformDbContext _context;

    /// <summary>Creates the repository.</summary>
    public SubscriptionAdjustmentRepository(PlatformDbContext context) => _context = context;

    /// <inheritdoc />
    public async Task AddAsync(SubscriptionAdjustment adjustment, CancellationToken cancellationToken = default) =>
        await _context.SubscriptionAdjustments.AddAsync(adjustment, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<SubscriptionAdjustment>> ListPendingByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        _context.SubscriptionAdjustments
            .Where(a => a.TenantId == tenantId && a.Status == AdjustmentStatus.PendingBilling)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<SubscriptionAdjustment>)t.Result, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<SubscriptionAdjustment>> ListByInvoiceIdAsync(
        Guid invoiceId,
        CancellationToken cancellationToken = default) =>
        _context.SubscriptionAdjustments
            .Where(a => a.BillingInvoiceId == invoiceId)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<SubscriptionAdjustment>)t.Result, cancellationToken);
}
