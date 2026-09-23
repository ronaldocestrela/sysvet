using SharedUI.Services;

namespace PlatformWeb.Services;

/// <summary>Platform operator auth state with Super Admin role tracking.</summary>
public sealed class PlatformAuthState : IPlatformAuthState
{
    private readonly ITokenStorage _tokenStorage;
    private string? _token;
    private string? _refreshToken;
    private IReadOnlyList<string> _menus = [];
    private IReadOnlyList<string> _roles = [];

    /// <summary>Creates state with isolated token storage.</summary>
    public PlatformAuthState(ITokenStorage tokenStorage) => _tokenStorage = tokenStorage;

    /// <inheritdoc />
    public bool IsAuthenticated => !string.IsNullOrEmpty(_token);

    /// <inheritdoc />
    public IReadOnlyList<string> Menus => _menus;

    /// <inheritdoc />
    public decimal MaxDiscountPercent => 0;

    /// <inheritdoc />
    public IReadOnlyList<string> Roles => _roles;

    /// <inheritdoc />
    public bool IsSuperAdmin => _roles.Contains(PlatformRoles.SuperAdmin, StringComparer.Ordinal);

    /// <inheritdoc />
    public event EventHandler? SessionChanged;

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        _token = await _tokenStorage.GetAccessTokenAsync();
        _refreshToken = await _tokenStorage.GetRefreshTokenAsync();
    }

    /// <inheritdoc />
    public Task<string?> GetTokenAsync() => Task.FromResult(_token);

    /// <inheritdoc />
    public Task<string?> GetRefreshTokenAsync() => Task.FromResult(_refreshToken);

    /// <inheritdoc />
    public async Task LoginAsync(string accessToken, string refreshToken)
    {
        _token = accessToken;
        _refreshToken = refreshToken;
        await _tokenStorage.SetTokensAsync(accessToken, refreshToken);
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
    public Task SetRolesAsync(IReadOnlyList<string> roles)
    {
        _roles = roles;
        SessionChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task LogoutAsync()
    {
        _token = null;
        _refreshToken = null;
        _menus = [];
        _roles = [];
        await _tokenStorage.ClearAsync();
        SessionChanged?.Invoke(this, EventArgs.Empty);
    }
}
