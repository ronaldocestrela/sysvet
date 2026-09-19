using Microsoft.JSInterop;
using SharedUI.Services;

namespace MauiApp.Services;

/// <summary>
/// Hybrid WebView download via the same JS helper as Blazor WASM.
/// </summary>
public sealed class MauiFileDownloadService : IFileDownloadService
{
    private readonly IJSRuntime _jsRuntime;

    public MauiFileDownloadService(IJSRuntime jsRuntime) => _jsRuntime = jsRuntime;

    public Task DownloadAsync(string fileName, string contentType, byte[] content, CancellationToken cancellationToken = default)
    {
        var base64 = Convert.ToBase64String(content);
        return _jsRuntime.InvokeVoidAsync("sysvetDownloadFile", fileName, contentType, base64).AsTask();
    }
}
