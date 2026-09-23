using Platform.Domain.Entities;

namespace Platform.Domain.Services;

/// <summary>Input row for invoice-based SaaS metrics (10.2).</summary>
public sealed record SaasInvoiceMetricsInput(
    Guid TenantId,
    DateTimeOffset PeriodStart,
    DateTimeOffset? PaidAt,
    DateTimeOffset? RefundedAt,
    decimal Amount,
    BillingInvoiceStatus Status);

/// <summary>Input row for tenant lifecycle in SaaS metrics (10.2).</summary>
public sealed record SaasTenantMetricsInput(
    Guid TenantId,
    string DisplayName,
    TenantStatus Status,
    DateTimeOffset? CancelledAt);

/// <summary>Input row for subscription and billing health (10.2).</summary>
public sealed record SaasSubscriptionMetricsInput(
    Guid TenantId,
    SubscriptionStatus Status,
    BillingStanding BillingStanding,
    DateTimeOffset? PastDueSince,
    decimal ContractedMrr);

/// <summary>Delinquent tenant or invoice line for the monthly report (10.2).</summary>
public sealed record SaasDelinquencyItem(
    Guid TenantId,
    string DisplayName,
    decimal OutstandingAmount,
    DateTimeOffset? PastDueSince,
    BillingStanding BillingStanding);

/// <summary>Aggregated SaaS metrics snapshot for one civil month (10.2).</summary>
public sealed record SaasMetricsSnapshot(
    int Year,
    int Month,
    decimal BilledMrr,
    decimal ContractedMrr,
    decimal Arr,
    decimal CashIn,
    decimal CashOut,
    decimal NetCashFlow,
    decimal LogoChurnRate,
    int PayingTenantsInMonth,
    int CancelledLogosInMonth,
    int PayingLogosAtMonthStart,
    decimal? Ltv,
    decimal AcquisitionSpend,
    int NewPayingTenantsInMonth,
    decimal? Cac,
    IReadOnlyList<SaasDelinquencyItem> Delinquency);

/// <summary>
/// Pure calculator for global VetNexus SaaS KPIs reconcilable with platform billing (10.2).
/// </summary>
public static class SaasMetricsCalculator
{
    /// <summary>Documented monetary tolerance for acceptance tests (BRL).</summary>
    public const decimal MoneyTolerance = 0.01m;

    /// <summary>Documented rate tolerance for acceptance tests.</summary>
    public const decimal RateTolerance = 0.0001m;

    /// <summary>Builds the metrics snapshot for the given civil month.</summary>
    public static SaasMetricsSnapshot Calculate(
        int year,
        int month,
        IReadOnlyList<SaasInvoiceMetricsInput> invoices,
        IReadOnlyList<SaasTenantMetricsInput> tenants,
        IReadOnlyList<SaasSubscriptionMetricsInput> subscriptions,
        decimal acquisitionSpendForMonth,
        IReadOnlyList<(Guid TenantId, DateTimeOffset FirstPaidAt)> firstPaidByTenant)
    {
        var (startUtc, endUtc) = SaasCivilMonthRange.ForMonth(year, month);
        var tenantById = tenants.ToDictionary(t => t.TenantId);
        var subscriptionByTenant = subscriptions.ToDictionary(s => s.TenantId);

        var mrrInvoices = invoices.Where(i =>
            SaasCivilMonthRange.Contains(i.PeriodStart, startUtc, endUtc) &&
            i.Status is BillingInvoiceStatus.Open or BillingInvoiceStatus.Failed or BillingInvoiceStatus.Paid).ToList();

        var billedMrr = mrrInvoices.Sum(i => i.Amount);

        var cashIn = invoices
            .Where(i => i.Status == BillingInvoiceStatus.Paid && i.PaidAt is not null &&
                        SaasCivilMonthRange.Contains(i.PaidAt.Value, startUtc, endUtc))
            .Sum(i => i.Amount);

        var cashOut = invoices
            .Where(i => i.Status == BillingInvoiceStatus.Refunded && i.RefundedAt is not null &&
                        SaasCivilMonthRange.Contains(i.RefundedAt.Value, startUtc, endUtc))
            .Sum(i => i.Amount);

        var payingTenantsInMonth = mrrInvoices.Select(i => i.TenantId).Distinct().Count();

        var contractedMrr = subscriptions
            .Where(s => s.Status == SubscriptionStatus.Active &&
                        tenantById.TryGetValue(s.TenantId, out var t) &&
                        t.Status is TenantStatus.Active or TenantStatus.Suspended)
            .Sum(s => s.ContractedMrr);

        var cancelledInMonth = tenants.Count(t =>
            t.CancelledAt is not null && SaasCivilMonthRange.Contains(t.CancelledAt.Value, startUtc, endUtc));

        var payingAtStartNow = CountPayingLogosAtInstant(tenantById, subscriptionByTenant, startUtc);
        var payingAtStart = payingAtStartNow + cancelledInMonth;

        var logoChurn = payingAtStart == 0 ? 0m : (decimal)cancelledInMonth / payingAtStart;

        decimal? ltv = null;
        if (logoChurn > RateTolerance && payingTenantsInMonth > 0)
        {
            var arpu = billedMrr / payingTenantsInMonth;
            ltv = arpu / logoChurn;
        }

        var newPaying = firstPaidByTenant.Count(f =>
            SaasCivilMonthRange.Contains(f.FirstPaidAt, startUtc, endUtc));

        decimal? cac = null;
        if (newPaying > 0 && acquisitionSpendForMonth >= 0)
        {
            cac = acquisitionSpendForMonth / newPaying;
        }

        var delinquency = BuildDelinquency(
            invoices,
            tenantById,
            subscriptionByTenant,
            startUtc,
            endUtc);

        return new SaasMetricsSnapshot(
            year,
            month,
            billedMrr,
            contractedMrr,
            billedMrr * 12,
            cashIn,
            cashOut,
            cashIn - cashOut,
            logoChurn,
            payingTenantsInMonth,
            cancelledInMonth,
            payingAtStart,
            ltv,
            acquisitionSpendForMonth,
            newPaying,
            cac,
            delinquency);
    }

    private static int CountPayingLogosAtInstant(
        IReadOnlyDictionary<Guid, SaasTenantMetricsInput> tenants,
        IReadOnlyDictionary<Guid, SaasSubscriptionMetricsInput> subscriptions,
        DateTimeOffset instantUtc)
    {
        var count = 0;
        foreach (var sub in subscriptions.Values)
        {
            if (sub.Status != SubscriptionStatus.Active)
            {
                continue;
            }

            if (!tenants.TryGetValue(sub.TenantId, out var tenant))
            {
                continue;
            }

            if (tenant.Status is not (TenantStatus.Active or TenantStatus.Suspended))
            {
                continue;
            }

            if (tenant.CancelledAt is not null && tenant.CancelledAt <= instantUtc)
            {
                continue;
            }

            count++;
        }

        return count;
    }

    private static IReadOnlyList<SaasDelinquencyItem> BuildDelinquency(
        IReadOnlyList<SaasInvoiceMetricsInput> invoices,
        IReadOnlyDictionary<Guid, SaasTenantMetricsInput> tenants,
        IReadOnlyDictionary<Guid, SaasSubscriptionMetricsInput> subscriptions,
        DateTimeOffset startUtc,
        DateTimeOffset endUtc)
    {
        var items = new Dictionary<Guid, SaasDelinquencyItem>();

        foreach (var invoice in invoices)
        {
            if (invoice.Status is not (BillingInvoiceStatus.Open or BillingInvoiceStatus.Failed))
            {
                continue;
            }

            if (!SaasCivilMonthRange.Contains(invoice.PeriodStart, startUtc, endUtc))
            {
                continue;
            }

            tenants.TryGetValue(invoice.TenantId, out var tenant);
            subscriptions.TryGetValue(invoice.TenantId, out var sub);
            var displayName = tenant?.DisplayName ?? invoice.TenantId.ToString();
            var standing = sub?.BillingStanding ?? BillingStanding.PastDue;
            var pastDue = sub?.PastDueSince;

            if (items.TryGetValue(invoice.TenantId, out var existing))
            {
                items[invoice.TenantId] = existing with
                {
                    OutstandingAmount = existing.OutstandingAmount + invoice.Amount
                };
            }
            else
            {
                items[invoice.TenantId] = new SaasDelinquencyItem(
                    invoice.TenantId,
                    displayName,
                    invoice.Amount,
                    pastDue,
                    standing);
            }
        }

        foreach (var sub in subscriptions.Values)
        {
            if (sub.BillingStanding is not (BillingStanding.PastDue or BillingStanding.Locked))
            {
                continue;
            }

            if (items.ContainsKey(sub.TenantId))
            {
                continue;
            }

            var outstanding = invoices
                .Where(i => i.TenantId == sub.TenantId && i.IsOutstanding())
                .Sum(i => i.Amount);

            if (outstanding <= 0)
            {
                continue;
            }

            tenants.TryGetValue(sub.TenantId, out var tenant);
            items[sub.TenantId] = new SaasDelinquencyItem(
                sub.TenantId,
                tenant?.DisplayName ?? sub.TenantId.ToString(),
                outstanding,
                sub.PastDueSince,
                sub.BillingStanding);
        }

        return items.Values.OrderByDescending(d => d.OutstandingAmount).ToList();
    }

    private static bool IsOutstanding(this SaasInvoiceMetricsInput invoice) =>
        invoice.Status is BillingInvoiceStatus.Open or BillingInvoiceStatus.Failed;
}
