using Microsoft.JSInterop;
using SharedUI.Services;

namespace PlatformWeb.Services;

/// <summary>Stores Super Admin JWT tokens in localStorage with keys isolated from other clients.</summary>
public sealed class WebTokenStorage : ITokenStorage
{
    private const string AccessKey = "platform_access_token";
    private const string RefreshKey = "platform_refresh_token";
    private readonly IJSRuntime _jsRuntime;

    /// <summary>Creates storage backed by the browser localStorage API.</summary>
    public WebTokenStorage(IJSRuntime jsRuntime) => _jsRuntime = jsRuntime;

    /// <inheritdoc />
    public Task<string?> GetAccessTokenAsync() =>
        _jsRuntime.InvokeAsync<string?>("sysvetAuth.getItem", AccessKey).AsTask();

    /// <inheritdoc />
    public Task<string?> GetRefreshTokenAsync() =>
        _jsRuntime.InvokeAsync<string?>("sysvetAuth.getItem", RefreshKey).AsTask();

    /// <inheritdoc />
    public async Task SetTokensAsync(string accessToken, string refreshToken)
    {
        await _jsRuntime.InvokeVoidAsync("sysvetAuth.setItem", AccessKey, accessToken);
        await _jsRuntime.InvokeVoidAsync("sysvetAuth.setItem", RefreshKey, refreshToken);
    }

    /// <inheritdoc />
    public async Task ClearAsync()
    {
        await _jsRuntime.InvokeVoidAsync("sysvetAuth.removeItem", AccessKey);
        await _jsRuntime.InvokeVoidAsync("sysvetAuth.removeItem", RefreshKey);
    }
}
