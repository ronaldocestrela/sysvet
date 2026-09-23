using FluentAssertions;
using Platform.Domain.Entities;
using Platform.Domain.Services;

namespace Platform.Tests.Domain;

public class DunningAndCouponTests
{
    [Fact]
    public void RecordPaymentOverdue_SetsPastDueSince_Once()
    {
        var start = DateTimeOffset.Parse("2026-01-01T00:00:00Z");
        var sub = TenantSubscription.CreateActive(Guid.NewGuid(), Guid.NewGuid(), start).Value;
        var first = DateTimeOffset.Parse("2026-01-10T00:00:00Z");
        var second = DateTimeOffset.Parse("2026-01-15T00:00:00Z");

        sub.RecordPaymentOverdue(first).IsSuccess.Should().BeTrue();
        sub.RecordPaymentOverdue(second).IsSuccess.Should().BeTrue();
        sub.PastDueSince.Should().Be(first);
        sub.BillingStanding.Should().Be(BillingStanding.PastDue);
    }

    [Fact]
    public void EvaluateOperationalLock_LocksAfterGraceDays()
    {
        var start = DateTimeOffset.Parse("2026-01-01T00:00:00Z");
        var sub = TenantSubscription.CreateActive(Guid.NewGuid(), Guid.NewGuid(), start).Value;
        var pastDue = DateTimeOffset.Parse("2026-01-05T00:00:00Z");
        sub.RecordPaymentOverdue(pastDue).IsSuccess.Should().BeTrue();

        sub.EvaluateOperationalLock(pastDue.AddDays(6), lockAfterDays: 7).IsSuccess.Should().BeTrue();
        sub.BillingStanding.Should().Be(BillingStanding.PastDue);

        sub.EvaluateOperationalLock(pastDue.AddDays(7), lockAfterDays: 7).IsSuccess.Should().BeTrue();
        sub.BillingStanding.Should().Be(BillingStanding.Locked);
        sub.IsOperationallyLocked.Should().BeTrue();
    }

    [Fact]
    public void RecordPaymentSuccess_ClearsLockAndPastDueSince()
    {
        var start = DateTimeOffset.Parse("2026-01-01T00:00:00Z");
        var sub = TenantSubscription.CreateActive(Guid.NewGuid(), Guid.NewGuid(), start).Value;
        var pastDue = DateTimeOffset.Parse("2026-01-05T00:00:00Z");
        sub.RecordPaymentOverdue(pastDue);
        sub.LockOperationalAccess(pastDue.AddDays(10));

        var paidAt = pastDue.AddDays(11);
        sub.RecordPaymentSuccess(paidAt).IsSuccess.Should().BeTrue();
        sub.BillingStanding.Should().Be(BillingStanding.Good);
        sub.PastDueSince.Should().BeNull();
        sub.IsOperationallyLocked.Should().BeFalse();
    }

    [Fact]
    public void DunningScheduleCalculator_ReturnsDueSteps_WhenNotSent()
    {
        var pastDue = DateTimeOffset.Parse("2026-01-01T00:00:00Z");
        var asOf = pastDue.AddDays(3);
        var sent = new HashSet<string>();

        var steps = DunningScheduleCalculator.GetDueSteps(pastDue, asOf, sent);

        steps.Should().Contain(s => s.StepDay == 0);
        steps.Should().Contain(s => s.StepDay == 3);
        steps.Should().NotContain(s => s.StepDay == 7);
    }

    [Fact]
    public void CardRetryPolicy_StopsAfterMaxRetries()
    {
        var invoice = BillingInvoice.Open(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(-30),
            DateTimeOffset.UtcNow,
            100m).Value;
        invoice.MarkFailed().IsSuccess.Should().BeTrue();

        var asOf = DateTimeOffset.UtcNow;
        CardRetryPolicy.ShouldRetry(invoice, asOf).Should().BeTrue();
        CardRetryPolicy.RecordAttemptScheduled(invoice, asOf);
        CardRetryPolicy.RecordAttemptScheduled(invoice, asOf);
        CardRetryPolicy.RecordAttemptScheduled(invoice, asOf);
        CardRetryPolicy.ShouldRetry(invoice, asOf.AddDays(2)).Should().BeFalse();
    }

    [Fact]
    public void Coupon_PercentDiscount_CapsAtSubtotal()
    {
        var coupon = Coupon.Create("SAVE10", CouponDiscountType.Percent, 10m, maxRedemptions: 5, expiresAt: null).Value;
        coupon.CalculateDiscount(200m).Should().Be(20m);
        coupon.CalculateDiscount(15m).Should().Be(1.5m);
    }

    [Fact]
    public void Coupon_FixedDiscount_DoesNotExceedSubtotal()
    {
        var coupon = Coupon.Create("FIX50", CouponDiscountType.FixedAmount, 50m, maxRedemptions: null, expiresAt: null).Value;
        coupon.CalculateDiscount(30m).Should().Be(30m);
    }

    [Fact]
    public void Coupon_CanRedeem_RejectsExpiredOrExhausted()
    {
        var expired = Coupon.Create(
            "OLD",
            CouponDiscountType.FixedAmount,
            10m,
            maxRedemptions: 1,
            expiresAt: DateTimeOffset.Parse("2026-01-01T00:00:00Z")).Value;
        expired.CanRedeem(DateTimeOffset.Parse("2026-02-01T00:00:00Z"), tenantAlreadyRedeemed: false).Should().BeFalse();

        var limited = Coupon.Create("ONE", CouponDiscountType.FixedAmount, 10m, maxRedemptions: 1, expiresAt: null).Value;
        limited.RecordRedemptionConsumed().IsSuccess.Should().BeTrue();
        limited.CanRedeem(DateTimeOffset.UtcNow, tenantAlreadyRedeemed: false).Should().BeFalse();
    }

    [Fact]
    public void ComposeAmount_AppliesCouponDiscount()
    {
        var tenantId = Guid.NewGuid();
        var coupon = Coupon.Create("PRO20", CouponDiscountType.Percent, 20m, maxRedemptions: null, expiresAt: null).Value;
        var total = BillingInvoiceComposer.ComposeAmount(100m, Array.Empty<decimal>(), Array.Empty<SubscriptionAdjustment>(), coupon);
        total.Should().Be(80m);
    }
}
