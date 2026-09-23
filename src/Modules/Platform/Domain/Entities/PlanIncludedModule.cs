using Core.Domain.Entitlements;

namespace Platform.Domain.Entities;

/// <summary>Join row between <see cref="Plan"/> and a <see cref="CommercialModule"/>.</summary>
public sealed class PlanIncludedModule
{
    /// <summary>Plan foreign key.</summary>
    public Guid PlanId { get; private set; }

    /// <summary>Included commercial module.</summary>
    public CommercialModule Module { get; private set; }

    /// <summary>Navigation to plan.</summary>
    public Plan? Plan { get; private set; }

#pragma warning disable CS8618
    private PlanIncludedModule()
    {
    }
#pragma warning restore CS8618

    /// <summary>Creates an inclusion row.</summary>
    public PlanIncludedModule(Guid planId, CommercialModule module)
    {
        PlanId = planId;
        Module = module;
    }
}
