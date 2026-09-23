using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions;
using Platform.Domain.Entities;
using Platform.Domain.Services;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Metrics;

/// <inheritdoc />
public sealed class SaasMetricsReader : ISaasMetricsReader
{
    private readonly PlatformDbContext _context;

    /// <summary>Creates the reader.</summary>
    public SaasMetricsReader(PlatformDbContext context) => _context = context;

    /// <inheritdoc />
    public async Task<SaasMetricsSnapshot> GetSnapshotAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        var invoices = await _context.BillingInvoices
            .AsNoTracking()
            .Select(i => new SaasInvoiceMetricsInput(
                i.TenantId,
                i.PeriodStart,
                i.PaidAt,
                i.RefundedAt,
                i.Amount,
                i.Status))
            .ToListAsync(cancellationToken);

        var tenants = await _context.Tenants
            .AsNoTracking()
            .Where(t => t.DeletedAt == null)
            .Select(t => new SaasTenantMetricsInput(
                t.Id,
                t.DisplayName,
                t.Status,
                t.CancelledAt))
            .ToListAsync(cancellationToken);

        var subscriptions = await _context.TenantSubscriptions
            .AsNoTracking()
            .Include(s => s.Plan)
            .Include(s => s.AddOns)
            .ThenInclude(a => a.AddOn)
            .ToListAsync(cancellationToken);

        var subscriptionInputs = subscriptions.Select(s =>
        {
            var addOnTotal = s.AddOns.Sum(a => a.AddOn?.MonthlyPrice ?? 0m);
            var planPrice = s.Plan?.MonthlyPrice ?? 0m;
            return new SaasSubscriptionMetricsInput(
                s.TenantId,
                s.Status,
                s.BillingStanding,
                s.PastDueSince,
                planPrice + addOnTotal);
        }).ToList();

        var acquisitionSpend = await _context.AcquisitionSpends
            .AsNoTracking()
            .Where(s => s.Year == year && s.Month == month)
            .SumAsync(s => s.Amount, cancellationToken);

        var paidRows = await _context.BillingInvoices
            .AsNoTracking()
            .Where(i => i.Status == BillingInvoiceStatus.Paid && i.PaidAt != null)
            .Select(i => new { i.TenantId, PaidAt = i.PaidAt!.Value })
            .ToListAsync(cancellationToken);

        var firstPaidTuples = paidRows
            .GroupBy(i => i.TenantId)
            .Select(g => (g.Key, g.Min(i => i.PaidAt)))
            .ToList();

        return SaasMetricsCalculator.Calculate(
            year,
            month,
            invoices,
            tenants,
            subscriptionInputs,
            acquisitionSpend,
            firstPaidTuples);
    }
}
