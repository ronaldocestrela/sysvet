using Core.Domain;
using Core.Domain.Entitlements;

namespace Platform.Domain.Entities;

/// <summary>Per-tenant commercial module override (dbo).</summary>
public sealed class FeatureFlag : Entity
{
    /// <summary>Owning tenant.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Target module.</summary>
    public CommercialModule Module { get; private set; }

    /// <summary>Override state.</summary>
    public FeatureFlagState State { get; private set; }

#pragma warning disable CS8618
    private FeatureFlag()
    {
    }
#pragma warning restore CS8618

    /// <summary>Creates or replaces semantics via repository upsert.</summary>
    public static Result<FeatureFlag> Create(Guid tenantId, CommercialModule module, FeatureFlagState state)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<FeatureFlag>(ErrorCodes.FeatureFlag.InvalidTenant);
        }

        return Result.Success(new FeatureFlag
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Module = module,
            State = state,
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = new byte[8]
        });
    }

    /// <summary>Updates override state.</summary>
    public Result SetState(FeatureFlagState state)
    {
        State = state;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
