namespace Commerce.Domain.Enums;

/// <summary>Fulfillment lifecycle for online orders.</summary>
public enum OnlineOrderStatus
{
    Placed = 0,
    Confirmed = 1,
    ReadyForPickup = 2,
    Completed = 3,
    Cancelled = 4
}
