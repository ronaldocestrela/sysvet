using Microsoft.JSInterop;
using SharedUI.Services;

namespace PlatformWeb.Services;

/// <summary>WASM download via data URL in the browser.</summary>
public sealed class WebFileDownloadService : IFileDownloadService
{
    private readonly IJSRuntime _jsRuntime;

    /// <summary>Creates the service.</summary>
    public WebFileDownloadService(IJSRuntime jsRuntime) => _jsRuntime = jsRuntime;

    /// <inheritdoc />
    public Task DownloadAsync(string fileName, string contentType, byte[] content, CancellationToken cancellationToken = default)
    {
        var base64 = Convert.ToBase64String(content);
        return _jsRuntime.InvokeVoidAsync("sysvetDownloadFile", fileName, contentType, base64).AsTask();
    }
}
