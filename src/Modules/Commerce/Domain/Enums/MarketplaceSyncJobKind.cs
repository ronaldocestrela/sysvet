namespace Commerce.Domain.Enums;

/// <summary>Marketplace outbox job type.</summary>
public enum MarketplaceSyncJobKind
{
    PushListing = 0,
    PullOrder = 1
}
