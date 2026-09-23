using FluentAssertions;
using Platform.Domain.Entities;
using Platform.Domain.Services;

namespace Platform.Tests.Domain;

public class SaasMetricsCalculatorTests
{
    private static readonly DateTimeOffset MarchMidUtc = new(2026, 3, 15, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset MarchPaidUtc = new(2026, 3, 20, 15, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset MarchRefundUtc = new(2026, 3, 25, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset MarchCancelUtc = new(2026, 3, 28, 18, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset FebPaidUtc = new(2026, 2, 10, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void SaasMetrics_MatchBilling_WithinDocumentedTolerance()
    {
        var cohort = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid()).ToList();
        var t1 = cohort[0];
        var t2 = cohort[1];
        var t3 = cohort[2];
        var churned = cohort[9];

        var invoices = new List<SaasInvoiceMetricsInput>
        {
            new(t1, MarchMidUtc, MarchPaidUtc, null, 100m, BillingInvoiceStatus.Paid),
            new(t2, MarchMidUtc, MarchPaidUtc, null, 200m, BillingInvoiceStatus.Paid),
            new(t3, MarchMidUtc, null, null, 300m, BillingInvoiceStatus.Open),
            new(t1, FebPaidUtc, FebPaidUtc, MarchRefundUtc, 50m, BillingInvoiceStatus.Refunded)
        };

        var tenants = cohort.Select(id => new SaasTenantMetricsInput(
            id,
            $"Tenant-{id:N}",
            id == churned ? TenantStatus.Cancelled : TenantStatus.Active,
            id == churned ? MarchCancelUtc : null)).ToList();

        var subscriptions = cohort.Select(id => new SaasSubscriptionMetricsInput(
            id,
            SubscriptionStatus.Active,
            id == t3 ? BillingStanding.PastDue : BillingStanding.Good,
            id == t3 ? MarchMidUtc : null,
            199m)).ToList();

        var firstPaid = new List<(Guid TenantId, DateTimeOffset FirstPaidAt)>
        {
            (t1, MarchPaidUtc),
            (t2, MarchPaidUtc),
            (Guid.NewGuid(), FebPaidUtc)
        };

        var snapshot = SaasMetricsCalculator.Calculate(
            2026,
            3,
            invoices,
            tenants,
            subscriptions,
            acquisitionSpendForMonth: 1000m,
            firstPaid);

        snapshot.BilledMrr.Should().BeApproximately(600m, SaasMetricsCalculator.MoneyTolerance);
        snapshot.Arr.Should().BeApproximately(7200m, SaasMetricsCalculator.MoneyTolerance);
        snapshot.CashIn.Should().BeApproximately(300m, SaasMetricsCalculator.MoneyTolerance);
        snapshot.CashOut.Should().BeApproximately(50m, SaasMetricsCalculator.MoneyTolerance);
        snapshot.NetCashFlow.Should().BeApproximately(250m, SaasMetricsCalculator.MoneyTolerance);
        snapshot.PayingTenantsInMonth.Should().Be(3);
        snapshot.CancelledLogosInMonth.Should().Be(1);
        snapshot.PayingLogosAtMonthStart.Should().Be(10);
        snapshot.LogoChurnRate.Should().BeApproximately(0.1m, SaasMetricsCalculator.RateTolerance);
        snapshot.Ltv.Should().NotBeNull();
        snapshot.Ltv!.Value.Should().BeApproximately(2000m, SaasMetricsCalculator.MoneyTolerance);
        snapshot.NewPayingTenantsInMonth.Should().Be(2);
        snapshot.Cac.Should().BeApproximately(500m, SaasMetricsCalculator.MoneyTolerance);
        snapshot.Delinquency.Should().Contain(d => d.TenantId == t3 && d.OutstandingAmount == 300m);
    }

    [Fact]
    public void Calculate_ReturnsNullLtvAndCac_WhenChurnOrDenominatorZero()
    {
        var tenantId = Guid.NewGuid();
        var tenants = new[] { new SaasTenantMetricsInput(tenantId, "Solo", TenantStatus.Active, null) };
        var subscriptions = new[]
        {
            new SaasSubscriptionMetricsInput(tenantId, SubscriptionStatus.Active, BillingStanding.Good, null, 199m)
        };

        var snapshot = SaasMetricsCalculator.Calculate(
            2026,
            3,
            Array.Empty<SaasInvoiceMetricsInput>(),
            tenants,
            subscriptions,
            500m,
            Array.Empty<(Guid, DateTimeOffset)>());

        snapshot.Ltv.Should().BeNull();
        snapshot.Cac.Should().BeNull();
        snapshot.LogoChurnRate.Should().Be(0m);
    }
}
