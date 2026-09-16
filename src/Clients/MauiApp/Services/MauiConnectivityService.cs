using SharedUI.Services;
using Microsoft.Maui.Networking;

namespace MauiApp.Services;

/// <summary>
/// MAUI connectivity adapter using <see cref="Connectivity"/>.
/// </summary>
public class MauiConnectivityService : IConnectivityService, IDisposable
{
    private ConnectivityStatus _status;

    /// <inheritdoc />
    public ConnectivityStatus Status => _status;

    /// <inheritdoc />
    public bool IsOnline => _status != ConnectivityStatus.Offline;

    /// <inheritdoc />
    public event EventHandler<ConnectivityStatus>? StatusChanged;

    /// <summary>Subscribes to MAUI connectivity change events.</summary>
    public MauiConnectivityService()
    {
        _status = Connectivity.Current.NetworkAccess == NetworkAccess.Internet
            ? ConnectivityStatus.Online
            : ConnectivityStatus.Offline;
        Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
    }

    /// <inheritdoc />
    public void SetSyncing(bool isSyncing)
    {
        if (_status == ConnectivityStatus.Offline)
        {
            return;
        }

        UpdateStatus(isSyncing ? ConnectivityStatus.Syncing : ConnectivityStatus.Online);
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        var isOnline = e.NetworkAccess == NetworkAccess.Internet;
        if (!isOnline)
        {
            UpdateStatus(ConnectivityStatus.Offline);
            return;
        }

        UpdateStatus(_status == ConnectivityStatus.Syncing ? ConnectivityStatus.Syncing : ConnectivityStatus.Online);
    }

    private void UpdateStatus(ConnectivityStatus status)
    {
        if (_status == status)
        {
            return;
        }

        _status = status;
        StatusChanged?.Invoke(this, _status);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;
    }
}
