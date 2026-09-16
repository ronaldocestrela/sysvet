namespace BlazorWeb.Services;

/// <summary>
/// Attempts to rotate JWT tokens using the refresh endpoint (no Bearer on that call).
/// </summary>
public interface IAuthTokenRefresher
{
    /// <summary>
    /// Calls <c>POST /api/v1/auth/refresh</c> and updates <see cref="SharedUI.Services.IAuthState"/> on success.
    /// </summary>
    Task<bool> TryRefreshAsync(CancellationToken cancellationToken = default);
}
