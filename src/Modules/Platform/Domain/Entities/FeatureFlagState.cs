namespace Platform.Domain.Entities;

/// <summary>Per-tenant override for a commercial module (9.3).</summary>
public enum FeatureFlagState
{
    /// <summary>Force-enable module even when not in plan.</summary>
    Enabled = 1,

    /// <summary>Force-disable module even when included in plan.</summary>
    Disabled = 2
}
