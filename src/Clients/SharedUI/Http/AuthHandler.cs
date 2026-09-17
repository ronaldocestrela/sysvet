using System.Net;
using System.Net.Http.Headers;
using SharedUI.Services;

namespace SharedUI.Http;

/// <summary>
/// Attaches Bearer tokens and retries once after a refresh on HTTP 401.
/// </summary>
public class AuthHandler : DelegatingHandler
{
    private readonly IAuthState _authState;
    private readonly IAuthTokenRefresher _tokenRefresher;

    /// <summary>Creates the handler with auth state and refresh support.</summary>
    public AuthHandler(IAuthState authState, IAuthTokenRefresher tokenRefresher)
    {
        _authState = authState;
        _tokenRefresher = tokenRefresher;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!IsAuthEndpoint(request))
        {
            await ApplyBearerAsync(request);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized && !IsAuthEndpoint(request))
        {
            response.Dispose();
            var refreshed = await _tokenRefresher.TryRefreshAsync(cancellationToken);
            if (refreshed)
            {
                request.Headers.Authorization = null;
                await ApplyBearerAsync(request);
                return await base.SendAsync(request, cancellationToken);
            }
        }

        return response;
    }

    private async Task ApplyBearerAsync(HttpRequestMessage request)
    {
        var token = await _authState.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private static bool IsAuthEndpoint(HttpRequestMessage request)
    {
        var path = request.RequestUri?.AbsolutePath ?? string.Empty;
        return path.Contains("/api/v1/auth/login", StringComparison.OrdinalIgnoreCase)
               || path.Contains("/api/v1/auth/refresh", StringComparison.OrdinalIgnoreCase);
    }
}
