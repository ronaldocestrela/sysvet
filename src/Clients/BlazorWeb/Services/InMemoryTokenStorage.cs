namespace BlazorWeb.Services;

/// <summary>
/// In-memory token storage for unit tests.
/// </summary>
public sealed class InMemoryTokenStorage : ITokenStorage
{
    private string? _access;
    private string? _refresh;

    /// <inheritdoc />
    public Task<string?> GetAccessTokenAsync() => Task.FromResult(_access);

    /// <inheritdoc />
    public Task<string?> GetRefreshTokenAsync() => Task.FromResult(_refresh);

    /// <inheritdoc />
    public Task SetTokensAsync(string accessToken, string refreshToken)
    {
        _access = accessToken;
        _refresh = refreshToken;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ClearAsync()
    {
        _access = null;
        _refresh = null;
        return Task.CompletedTask;
    }
}
