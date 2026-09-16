namespace SharedUI.Services;

/// <summary>
/// Network and sync indicator state for client hosts.
/// </summary>
public enum ConnectivityStatus
{
    /// <summary>Browser or device reports network connectivity.</summary>
    Online = 0,

    /// <summary>No network; PWA may still serve cached routes.</summary>
    Offline = 1,

    /// <summary>Reserved for background sync (Fase 3.5).</summary>
    Syncing = 2
}
