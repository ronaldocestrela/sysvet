using Core.Domain.Entitlements;
using FluentAssertions;
using Platform.Domain.Entities;
using Platform.Domain.Services;

namespace Platform.Tests.Domain;

public class EntitlementCalculatorTests
{
    [Fact]
    public void ComputeEffective_StarterPlan_IncludesVeterinaryOnly()
    {
        var effective = EntitlementCalculator.ComputeEffective(
            SubscriptionStatus.Active,
            [CommercialModule.Veterinary],
            [],
            []);

        effective.Should().BeEquivalentTo(new[] { CommercialModule.Veterinary });
    }

    [Fact]
    public void ComputeEffective_FlagDisabled_RemovesPlanModule()
    {
        var effective = EntitlementCalculator.ComputeEffective(
            SubscriptionStatus.Active,
            [CommercialModule.Veterinary, CommercialModule.Fiscal],
            [],
            [(CommercialModule.Fiscal, FeatureFlagState.Disabled)]);

        effective.Should().BeEquivalentTo(new[] { CommercialModule.Veterinary });
    }

    [Fact]
    public void ComputeEffective_FlagEnabled_AddsModuleOutsidePlan()
    {
        var effective = EntitlementCalculator.ComputeEffective(
            SubscriptionStatus.Active,
            [CommercialModule.Veterinary],
            [],
            [(CommercialModule.Fiscal, FeatureFlagState.Enabled)]);

        effective.Should().BeEquivalentTo(new[] { CommercialModule.Veterinary, CommercialModule.Fiscal });
    }

    [Fact]
    public void ComputeEffective_AddOn_UnionWithPlan()
    {
        var effective = EntitlementCalculator.ComputeEffective(
            SubscriptionStatus.Active,
            [CommercialModule.Veterinary],
            [CommercialModule.Petshop],
            []);

        effective.Should().Contain(CommercialModule.Petshop);
    }

    [Fact]
    public void ComputeEffective_TrialExpired_ReturnsEmpty()
    {
        var effective = EntitlementCalculator.ComputeEffective(
            SubscriptionStatus.TrialExpired,
            [CommercialModule.Veterinary],
            [CommercialModule.Fiscal],
            [(CommercialModule.Sales, FeatureFlagState.Enabled)]);

        effective.Should().BeEmpty();
    }
}
