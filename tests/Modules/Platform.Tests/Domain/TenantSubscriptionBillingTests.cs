using FluentAssertions;
using Platform.Domain.Entities;
using Platform.Domain.Services;

namespace Platform.Tests.Domain;

public class TenantSubscriptionBillingTests
{
    [Fact]
    public void IsDueForBilling_WhenActiveAndPeriodEndReached_ReturnsTrue()
    {
        var start = DateTimeOffset.Parse("2026-01-01T00:00:00Z");
        var sub = TenantSubscription.CreateActive(Guid.NewGuid(), Guid.NewGuid(), start).Value;

        sub.IsDueForBilling(start.AddDays(ProrationCalculator.DefaultPeriodDays)).Should().BeTrue();
    }

    [Fact]
    public void IsDueForBilling_WhenTrial_DoesNotCharge()
    {
        var start = DateTimeOffset.Parse("2026-01-01T00:00:00Z");
        var sub = TenantSubscription.CreateTrial(Guid.NewGuid(), Guid.NewGuid(), start, 14, TrialEndAction.Block).Value;

        sub.IsDueForBilling(start.AddDays(30)).Should().BeFalse();
    }

    [Fact]
    public void RecordPaymentSuccess_AdvancesPeriodAndSetsGood()
    {
        var start = DateTimeOffset.Parse("2026-01-01T00:00:00Z");
        var sub = TenantSubscription.CreateActive(Guid.NewGuid(), Guid.NewGuid(), start).Value;
        var asOf = start.AddDays(ProrationCalculator.DefaultPeriodDays);

        sub.RecordPaymentSuccess(asOf).IsSuccess.Should().BeTrue();
        sub.BillingStanding.Should().Be(BillingStanding.Good);
        sub.PeriodStart.Should().Be(asOf);
        sub.PeriodEnd.Should().Be(asOf.AddDays(ProrationCalculator.DefaultPeriodDays));
    }

    [Fact]
    public void RecordPaymentOverdue_SetsPastDueWithoutCancelingSubscriptionStatus()
    {
        var start = DateTimeOffset.Parse("2026-01-01T00:00:00Z");
        var sub = TenantSubscription.CreateActive(Guid.NewGuid(), Guid.NewGuid(), start).Value;

        sub.RecordPaymentOverdue(start.AddDays(35)).IsSuccess.Should().BeTrue();
        sub.BillingStanding.Should().Be(BillingStanding.PastDue);
        sub.Status.Should().Be(SubscriptionStatus.Active);
    }

    [Fact]
    public void CancelBilling_PreventsDueBilling()
    {
        var start = DateTimeOffset.Parse("2026-01-01T00:00:00Z");
        var sub = TenantSubscription.CreateActive(Guid.NewGuid(), Guid.NewGuid(), start).Value;
        sub.CancelBilling(start).IsSuccess.Should().BeTrue();

        sub.IsDueForBilling(start.AddDays(30)).Should().BeFalse();
    }

    [Fact]
    public void Adjustment_MarkInvoicedThenSettled_TransitionsStatus()
    {
        var adjustment = SubscriptionAdjustment.CreatePending(Guid.NewGuid(), 10m, null, Guid.NewGuid()).Value;
        var invoiceId = Guid.NewGuid();

        adjustment.MarkInvoiced(invoiceId).IsSuccess.Should().BeTrue();
        adjustment.Status.Should().Be(AdjustmentStatus.Invoiced);
        adjustment.MarkSettled().IsSuccess.Should().BeTrue();
        adjustment.Status.Should().Be(AdjustmentStatus.Settled);
    }

    [Fact]
    public void BillingInvoice_MarkPaid_IsIdempotent()
    {
        var invoice = BillingInvoice.Open(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(-30),
            DateTimeOffset.UtcNow,
            100m).Value;

        invoice.MarkPaid(DateTimeOffset.UtcNow).IsSuccess.Should().BeTrue();
        invoice.MarkPaid(DateTimeOffset.UtcNow).IsSuccess.Should().BeTrue();
        invoice.Status.Should().Be(BillingInvoiceStatus.Paid);
    }
}
