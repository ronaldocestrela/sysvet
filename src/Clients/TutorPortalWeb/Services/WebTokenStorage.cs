using Microsoft.JSInterop;
using SharedUI.Services;

namespace TutorPortalWeb.Services;

/// <summary>
/// Stores tutor portal JWT tokens in <c>localStorage</c> with keys isolated from the clinic app.
/// </summary>
public sealed class WebTokenStorage : ITokenStorage
{
    private const string AccessKey = "tutorportal_access_token";
    private const string RefreshKey = "tutorportal_refresh_token";
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
