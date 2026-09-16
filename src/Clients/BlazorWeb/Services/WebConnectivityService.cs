using SharedUI.Services;
using Microsoft.JSInterop;

namespace BlazorWeb.Services;

/// <summary>
/// WASM connectivity detection via browser online/offline events.
/// </summary>
public class WebConnectivityService : IConnectivityService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private DotNetObjectReference<WebConnectivityService>? _objRef;
    private ConnectivityStatus _status = ConnectivityStatus.Online;

    /// <inheritdoc />
    public ConnectivityStatus Status => _status;

    /// <inheritdoc />
    public bool IsOnline => _status != ConnectivityStatus.Offline;

    /// <inheritdoc />
    public event EventHandler<ConnectivityStatus>? StatusChanged;

    /// <summary>Creates the service with JS interop for browser events.</summary>
    public WebConnectivityService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>Registers browser listeners; call once at startup.</summary>
    public async Task InitializeAsync()
    {
        _objRef = DotNetObjectReference.Create(this);
        var isOnline = await _jsRuntime.InvokeAsync<bool>("eval", "navigator.onLine");
        SetStatus(isOnline ? ConnectivityStatus.Online : ConnectivityStatus.Offline);
        await _jsRuntime.InvokeVoidAsync("registerConnectivityListeners", _objRef);
    }

    /// <inheritdoc />
    public void SetSyncing(bool isSyncing)
    {
        if (_status == ConnectivityStatus.Offline)
        {
            return;
        }

        SetStatus(isSyncing ? ConnectivityStatus.Syncing : ConnectivityStatus.Online);
    }

    /// <summary>Callback from JavaScript when connectivity changes.</summary>
    [JSInvokable]
    public void UpdateStatus(bool isOnline)
    {
        if (!isOnline)
        {
            SetStatus(ConnectivityStatus.Offline);
            return;
        }

        SetStatus(_status == ConnectivityStatus.Syncing ? ConnectivityStatus.Syncing : ConnectivityStatus.Online);
    }

    private void SetStatus(ConnectivityStatus status)
    {
        if (_status == status)
        {
            return;
        }

        _status = status;
        StatusChanged?.Invoke(this, _status);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_objRef != null)
        {
            await _jsRuntime.InvokeVoidAsync("unregisterConnectivityListeners");
            _objRef.Dispose();
        }
    }
}
