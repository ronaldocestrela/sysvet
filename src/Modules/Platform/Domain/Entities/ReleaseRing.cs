namespace Platform.Domain.Entities;

/// <summary>
/// Operational release ring for staged rollout (ADR-060); not a commercial feature flag.
/// </summary>
public enum ReleaseRing
{
    /// <summary>First production slice (internal or pilot tenant).</summary>
    Canary = 0,

    /// <summary>Expanded beta cohort before general availability.</summary>
    Beta = 1,

    /// <summary>Default ring for production tenants.</summary>
    GeneralAvailability = 2
}
