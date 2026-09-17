namespace Clients.Infrastructure.Sync;

/// <summary>
/// Host-provided online/sync UI state for <see cref="SyncBackgroundWorker"/>.
/// </summary>
public interface ISyncConnectivity
{
    /// <summary>Whether API calls should be attempted.</summary>
    bool IsOnline { get; }

    /// <summary>Raises when connectivity transitions to online.</summary>
    event EventHandler? OnlineStateChanged;

    /// <summary>Updates the syncing badge while online.</summary>
    void SetSyncing(bool isSyncing);
}
