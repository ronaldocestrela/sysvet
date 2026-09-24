namespace Platform.Domain.Entities;

/// <summary>Customer-visible severity for the public status page (ADR-060).</summary>
public enum StatusIncidentImpact
{
    /// <summary>No user-visible impact.</summary>
    None = 0,

    /// <summary>Degraded experience for a subset of features.</summary>
    Minor = 1,

    /// <summary>Widespread degradation.</summary>
    Major = 2,

    /// <summary>Core functionality unavailable.</summary>
    Critical = 3
}
