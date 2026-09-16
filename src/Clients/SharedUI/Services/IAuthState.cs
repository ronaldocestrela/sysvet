namespace SharedUI.Services;

/// <summary>
/// Host-specific authentication state (JWT in memory, secure storage, etc.).
/// </summary>
public interface IAuthState
{
    /// <summary>Whether the user has a valid access token.</summary>
    bool IsAuthenticated { get; }

    /// <summary>Menu keys from the last <c>/auth/me</c> response.</summary>
    IReadOnlyList<string> Menus { get; }

    /// <summary>Raised when tokens or menus change (login, logout, refresh).</summary>
    event EventHandler? SessionChanged;

    /// <summary>Loads persisted tokens from host storage (call once at startup).</summary>
    Task InitializeAsync();

    /// <summary>Returns the current bearer token, if any.</summary>
    Task<string?> GetTokenAsync();

    /// <summary>Returns the current refresh token, if any.</summary>
    Task<string?> GetRefreshTokenAsync();

    /// <summary>Persists tokens after successful login or refresh.</summary>
    Task LoginAsync(string accessToken, string refreshToken);

    /// <summary>Updates menu keys after loading the user profile.</summary>
    Task SetMenusAsync(IReadOnlyList<string> menus);

    /// <summary>Clears local session state.</summary>
    Task LogoutAsync();
}
