using Clients.Infrastructure.Sync;
using SharedUI.Services;

namespace SharedUI.Services;

/// <summary>
/// Bridges SharedUI connectivity to the sync worker in Clients.Infrastructure.
/// </summary>
public sealed class SyncConnectivityAdapter : ISyncConnectivity
{
    private readonly IConnectivityService _connectivityService;

    /// <summary>Creates the adapter.</summary>
    public SyncConnectivityAdapter(IConnectivityService connectivityService)
    {
        _connectivityService = connectivityService;
        _connectivityService.StatusChanged += OnStatusChanged;
    }

    /// <inheritdoc />
    public bool IsOnline => _connectivityService.IsOnline;

    /// <inheritdoc />
    public event EventHandler? OnlineStateChanged;

    /// <inheritdoc />
    public void SetSyncing(bool isSyncing) => _connectivityService.SetSyncing(isSyncing);

    private void OnStatusChanged(object? sender, ConnectivityStatus status)
    {
        if (status == ConnectivityStatus.Online)
        {
            OnlineStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
