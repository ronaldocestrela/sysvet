namespace Inventory.Domain.Enums;

/// <summary>
/// Accounts payable linkage state until Finance module consumes the import event.
/// </summary>
public enum ApIntegrationStatus
{
    Pending = 0,
    Linked = 1
}
