using Core.Domain.Entitlements;
using FluentAssertions;
using Platform.Domain.Entities;
using Platform.Domain.Services;

namespace Platform.Tests.Domain;

public class ModuleAdoptionCalculatorTests
{
    [Fact]
    public void Calculate_FlagEnabled_IncludesModuleOutsidePlan()
    {
        var t1 = Guid.NewGuid();
        var effective = EntitlementCalculator.ComputeEffective(
            SubscriptionStatus.Active,
            [CommercialModule.Veterinary],
            [],
            [(CommercialModule.Fiscal, FeatureFlagState.Enabled)]);

        var snapshot = ModuleAdoptionCalculator.Calculate(
        [
            new TenantModuleAdoptionInput(t1, "Clinic A", effective)
        ]);

        snapshot.Tenants.Should().ContainSingle();
        snapshot.Tenants[0].Modules[CommercialModule.Fiscal].Should().BeTrue();
        snapshot.ModuleSummaries.Single(s => s.Module == CommercialModule.Fiscal).EnabledCount.Should().Be(1);
    }

    [Fact]
    public void Calculate_FlagDisabled_RemovesModule()
    {
        var t1 = Guid.NewGuid();
        var effective = EntitlementCalculator.ComputeEffective(
            SubscriptionStatus.Active,
            [CommercialModule.Veterinary, CommercialModule.Fiscal],
            [],
            [(CommercialModule.Fiscal, FeatureFlagState.Disabled)]);

        var snapshot = ModuleAdoptionCalculator.Calculate(
        [
            new TenantModuleAdoptionInput(t1, "Clinic A", effective)
        ]);

        snapshot.Tenants[0].Modules[CommercialModule.Fiscal].Should().BeFalse();
    }

    [Fact]
    public void Calculate_TrialExpired_EmptyModules()
    {
        var t1 = Guid.NewGuid();
        var effective = EntitlementCalculator.ComputeEffective(
            SubscriptionStatus.TrialExpired,
            [CommercialModule.Veterinary],
            [CommercialModule.Petshop],
            []);

        var snapshot = ModuleAdoptionCalculator.Calculate(
        [
            new TenantModuleAdoptionInput(t1, "Clinic A", effective)
        ]);

        snapshot.ModuleSummaries.All(s => s.EnabledCount == 0).Should().BeTrue();
    }
}
