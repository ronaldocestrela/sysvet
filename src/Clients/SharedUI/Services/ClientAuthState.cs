namespace SharedUI.Services;

/// <summary>
/// Host-agnostic authentication state backed by <see cref="ITokenStorage"/>.
/// </summary>
public sealed class ClientAuthState : IAuthState
{
    private readonly ITokenStorage _tokenStorage;
    private string? _token;
    private string? _refreshToken;
    private IReadOnlyList<string> _menus = [];
    private decimal _maxDiscountPercent;

    /// <summary>Creates state with the host token storage implementation.</summary>
    public ClientAuthState(ITokenStorage tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    /// <inheritdoc />
    public bool IsAuthenticated => !string.IsNullOrEmpty(_token);

    /// <inheritdoc />
    public IReadOnlyList<string> Menus => _menus;

    /// <inheritdoc />
    public decimal MaxDiscountPercent => _maxDiscountPercent;

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

    /// <summary>Stores profile discount ceiling from <c>/auth/me</c>.</summary>
    public Task SetMaxDiscountPercentAsync(decimal maxDiscountPercent)
    {
        _maxDiscountPercent = maxDiscountPercent;
        SessionChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task LogoutAsync()
    {
        _token = null;
        _refreshToken = null;
        _menus = [];
        _maxDiscountPercent = 0;
        await _tokenStorage.ClearAsync();
        SessionChanged?.Invoke(this, EventArgs.Empty);
    }
}
