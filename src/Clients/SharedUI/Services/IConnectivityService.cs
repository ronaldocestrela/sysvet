namespace SharedUI.Services;

/// <summary>
/// Host-specific online/offline detection for banners and badges.
/// </summary>
public interface IConnectivityService
{
    /// <summary>Current connectivity or sync state.</summary>
    ConnectivityStatus Status { get; }

    /// <summary>Whether the host considers itself online for API calls.</summary>
    bool IsOnline { get; }

    /// <summary>Raised when <see cref="Status"/> changes.</summary>
    event EventHandler<ConnectivityStatus>? StatusChanged;

    /// <summary>
    /// Sets sync state while online (no-op when offline). Used by the sync engine in 3.5.
    /// </summary>
    void SetSyncing(bool isSyncing);
}
