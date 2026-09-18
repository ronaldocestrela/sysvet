namespace Inventory.Domain.Services;

/// <summary>
/// Classification of inventory alert rows for list projections.
/// </summary>
public enum StockAlertKind
{
    None = 0,
    LowStock = 1,
    Expired = 2,
    ExpiringSoon = 3
}
