namespace Platform.Domain.Entities;

/// <summary>Behavior when a tenant trial period ends (9.3).</summary>
public enum TrialEndAction
{
    /// <summary>Disable commercial modules until billing (9.5 payment screen).</summary>
    Block = 1,

    /// <summary>Convert to active subscription without blocking modules.</summary>
    Convert = 2
}
