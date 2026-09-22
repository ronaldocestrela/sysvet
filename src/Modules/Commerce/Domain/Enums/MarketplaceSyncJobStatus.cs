namespace Commerce.Domain.Enums;

/// <summary>Processing state for marketplace sync jobs.</summary>
public enum MarketplaceSyncJobStatus
{
    Pending = 0,
    Processing = 1,
    Succeeded = 2,
    DeadLetter = 3
}
