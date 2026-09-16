namespace BlazorWeb.Services;

/// <summary>
/// Persists JWT material in browser storage for the WASM host.
/// </summary>
public interface ITokenStorage
{
    /// <summary>Reads the stored access token.</summary>
    Task<string?> GetAccessTokenAsync();

    /// <summary>Reads the stored refresh token.</summary>
    Task<string?> GetRefreshTokenAsync();

    /// <summary>Stores both tokens after login or refresh.</summary>
    Task SetTokensAsync(string accessToken, string refreshToken);

    /// <summary>Removes all stored tokens.</summary>
    Task ClearAsync();
}
