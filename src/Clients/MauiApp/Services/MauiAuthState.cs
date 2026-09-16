using SharedUI.Services;

namespace MauiApp.Services;

/// <summary>
/// MAUI authentication state using secure storage for tokens.
/// </summary>
public class MauiAuthState : IAuthState
{
    private const string AccessKey = "jwt_token";
    private const string RefreshKey = "jwt_refresh_token";
    private string? _token;
    private string? _refreshToken;
    private IReadOnlyList<string> _menus = [];

    /// <inheritdoc />
    public bool IsAuthenticated => !string.IsNullOrEmpty(_token);

    /// <inheritdoc />
    public IReadOnlyList<string> Menus => _menus;

    /// <inheritdoc />
    public event EventHandler? SessionChanged;

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        _token = await SecureStorage.Default.GetAsync(AccessKey);
        _refreshToken = await SecureStorage.Default.GetAsync(RefreshKey);
    }

    /// <inheritdoc />
    public async Task<string?> GetTokenAsync()
    {
        if (string.IsNullOrEmpty(_token))
        {
            _token = await SecureStorage.Default.GetAsync(AccessKey);
        }

        return _token;
    }

    /// <inheritdoc />
    public async Task<string?> GetRefreshTokenAsync()
    {
        if (string.IsNullOrEmpty(_refreshToken))
        {
            _refreshToken = await SecureStorage.Default.GetAsync(RefreshKey);
        }

        return _refreshToken;
    }

    /// <inheritdoc />
    public async Task LoginAsync(string accessToken, string refreshToken)
    {
        _token = accessToken;
        _refreshToken = refreshToken;
        await SecureStorage.Default.SetAsync(AccessKey, accessToken);
        await SecureStorage.Default.SetAsync(RefreshKey, refreshToken);
        SessionChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public Task SetMenusAsync(IReadOnlyList<string> menus)
    {
        _menus = menus;
        SessionChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task LogoutAsync()
    {
        _token = null;
        _refreshToken = null;
        _menus = [];
        SecureStorage.Default.Remove(AccessKey);
        SecureStorage.Default.Remove(RefreshKey);
        SessionChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }
}
