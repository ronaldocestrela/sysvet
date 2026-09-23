namespace Core.Application.Sync;

/// <summary>Observes server-side sync push failures for operational alerting (10.5).</summary>
public interface ISyncPushObserver
{
    /// <summary>Called when a push batch stops on a failed outbox message.</summary>
    void OnPushFailure(Guid tenantId, Guid messageId, string errorCode, bool isPermanentFailure);
}
