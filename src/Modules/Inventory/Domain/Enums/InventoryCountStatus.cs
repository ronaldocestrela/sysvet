namespace Inventory.Domain.Enums;

/// <summary>
/// Lifecycle of a physical inventory count session.
/// </summary>
public enum InventoryCountStatus
{
    /// <summary>Blind counting in progress; expected quantities are hidden.</summary>
    InProgress = 0,

    /// <summary>Submitted for review; variances are visible.</summary>
    Submitted = 1,

    /// <summary>Approved and stock adjustments applied.</summary>
    Approved = 2,

    /// <summary>Cancelled without stock impact.</summary>
    Cancelled = 3
}
