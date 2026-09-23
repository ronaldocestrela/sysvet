using FluentAssertions;
using Platform.Domain.Entities;
using Platform.Domain.Services;

namespace Platform.Tests.Domain;

public class ProrationCalculatorTests
{
    [Fact]
    public void CalculateDelta_UpgradeMidCycle_ReturnsPositiveCharge()
    {
        var delta = ProrationCalculator.CalculateDelta(199m, 399m, 15);
        delta.Should().Be(100m);
    }

    [Fact]
    public void CalculateDelta_DowngradeMidCycle_ReturnsNegativeCredit()
    {
        var delta = ProrationCalculator.CalculateDelta(399m, 199m, 15);
        delta.Should().Be(-100m);
    }

    [Fact]
    public void CalculateDelta_LastDay_ReturnsMinimalDelta()
    {
        var delta = ProrationCalculator.CalculateDelta(199m, 399m, 1);
        delta.Should().BeApproximately(6.67m, 0.01m);
    }

    [Fact]
    public void ChangePlan_AppliesCredit_OnUpgrade()
    {
        var start = DateTimeOffset.Parse("2026-01-01T00:00:00Z");
        var sub = Platform.Domain.Entities.TenantSubscription.CreateActive(Guid.NewGuid(), Guid.NewGuid(), start).Value;
        sub.ApplyAddOnPriceDelta(-50m).IsSuccess.Should().BeTrue();

        var result = sub.ChangePlan(Guid.NewGuid(), 199m, 399m, start.AddDays(15));
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(50m);
        sub.CreditBalance.Should().Be(0m);
    }

    [Fact]
    public void ExpireTrial_Block_SetsTrialExpired()
    {
        var start = DateTimeOffset.Parse("2026-01-01T00:00:00Z");
        var sub = Platform.Domain.Entities.TenantSubscription.CreateTrial(
            Guid.NewGuid(),
            Guid.NewGuid(),
            start,
            trialDays: 7,
            TrialEndAction.Block).Value;

        var result = sub.ExpireTrial(start.AddDays(8));
        result.IsSuccess.Should().BeTrue();
        sub.Status.Should().Be(Platform.Domain.Entities.SubscriptionStatus.TrialExpired);
    }

    [Fact]
    public void ExpireTrial_Convert_SetsActive()
    {
        var start = DateTimeOffset.Parse("2026-01-01T00:00:00Z");
        var sub = Platform.Domain.Entities.TenantSubscription.CreateTrial(
            Guid.NewGuid(),
            Guid.NewGuid(),
            start,
            trialDays: 7,
            TrialEndAction.Convert).Value;

        var result = sub.ExpireTrial(start.AddDays(8));
        result.IsSuccess.Should().BeTrue();
        sub.Status.Should().Be(Platform.Domain.Entities.SubscriptionStatus.Active);
        sub.TrialEndsAt.Should().BeNull();
    }
}
