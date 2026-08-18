using SharedUI.Services;
using Microsoft.JSInterop;

namespace BlazorWeb.Services;

public class WebConnectivityService : IConnectivityService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private DotNetObjectReference<WebConnectivityService>? _objRef;
    private bool _isOnline = true;

    public bool IsOnline => _isOnline;
    public event EventHandler<bool>? ConnectivityChanged;

    public WebConnectivityService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        _objRef = DotNetObjectReference.Create(this);
        _isOnline = await _jsRuntime.InvokeAsync<bool>("eval", "navigator.onLine");
        await _jsRuntime.InvokeVoidAsync("registerConnectivityListeners", _objRef);
    }

    [JSInvokable]
    public void UpdateStatus(bool isOnline)
    {
        if (_isOnline != isOnline)
        {
            _isOnline = isOnline;
            ConnectivityChanged?.Invoke(this, _isOnline);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_objRef != null)
        {
            await _jsRuntime.InvokeVoidAsync("unregisterConnectivityListeners");
            _objRef.Dispose();
        }
    }
}
