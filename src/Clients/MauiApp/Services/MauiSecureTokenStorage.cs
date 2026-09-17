using SharedUI.Services;

namespace MauiApp.Services;

/// <summary>
/// Persists JWT tokens in MAUI <see cref="SecureStorage"/>.
/// </summary>
public sealed class MauiSecureTokenStorage : ITokenStorage
{
    private const string AccessKey = "sysvet_access_token";
    private const string RefreshKey = "sysvet_refresh_token";

    /// <inheritdoc />
    public Task<string?> GetAccessTokenAsync() => SecureStorage.Default.GetAsync(AccessKey);

    /// <inheritdoc />
    public Task<string?> GetRefreshTokenAsync() => SecureStorage.Default.GetAsync(RefreshKey);

    /// <inheritdoc />
    public async Task SetTokensAsync(string accessToken, string refreshToken)
    {
        await SecureStorage.Default.SetAsync(AccessKey, accessToken);
        await SecureStorage.Default.SetAsync(RefreshKey, refreshToken);
    }

    /// <inheritdoc />
    public Task ClearAsync()
    {
        SecureStorage.Default.Remove(AccessKey);
        SecureStorage.Default.Remove(RefreshKey);
        return Task.CompletedTask;
    }
}
