using Core.Domain.Entitlements;
using Platform.Domain.Entities;

namespace Platform.Domain.Services;

/// <summary>
/// Pure domain rules for effective commercial modules from plan, add-ons and flags.
/// </summary>
public static class EntitlementCalculator
{
    /// <summary>
    /// Computes enabled modules: plan ∪ add-ons, then apply flag overrides; trial expired yields empty set.
    /// </summary>
    public static IReadOnlySet<CommercialModule> ComputeEffective(
        SubscriptionStatus status,
        IEnumerable<CommercialModule> planModules,
        IEnumerable<CommercialModule> activeAddOnModules,
        IEnumerable<(CommercialModule Module, FeatureFlagState State)> featureFlags)
    {
        if (status == SubscriptionStatus.TrialExpired)
        {
            return new HashSet<CommercialModule>();
        }

        var effective = new HashSet<CommercialModule>(planModules);
        foreach (var module in activeAddOnModules)
        {
            effective.Add(module);
        }

        foreach (var (module, state) in featureFlags)
        {
            if (state == FeatureFlagState.Disabled)
            {
                effective.Remove(module);
            }
            else
            {
                effective.Add(module);
            }
        }

        return effective;
    }
}
