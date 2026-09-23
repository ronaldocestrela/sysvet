namespace Core.Application.Sync;

/// <summary>No-op observer used when operational alerting is not composed.</summary>
public sealed class NullSyncPushObserver : ISyncPushObserver
{
    /// <inheritdoc />
    public void OnPushFailure(Guid tenantId, Guid messageId, string errorCode, bool isPermanentFailure)
    {
    }
}
