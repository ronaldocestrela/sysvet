using SharedUI.Services;
using Microsoft.Maui.Networking;

namespace MauiApp.Services;

public class MauiConnectivityService : IConnectivityService, IDisposable
{
    private bool _isOnline;

    public bool IsOnline => _isOnline;
    public event EventHandler<bool>? ConnectivityChanged;

    public MauiConnectivityService()
    {
        _isOnline = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
        Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        var isOnline = e.NetworkAccess == NetworkAccess.Internet;
        if (_isOnline != isOnline)
        {
            _isOnline = isOnline;
            ConnectivityChanged?.Invoke(this, _isOnline);
        }
    }

    public void Dispose()
    {
        Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;
    }
}
