namespace SharedUI.Services;

/// <summary>
/// Host-specific authentication state (JWT in memory, secure storage, etc.).
/// </summary>
public interface IAuthState
{
    /// <summary>Whether the user has a valid access token.</summary>
    bool IsAuthenticated { get; }

    /// <summary>Returns the current bearer token, if any.</summary>
    Task<string?> GetTokenAsync();

    /// <summary>Persists a token after successful login.</summary>
    /// <param name="token">JWT access token.</param>
    Task LoginAsync(string token);

    /// <summary>Clears local session state.</summary>
    Task LogoutAsync();
}
