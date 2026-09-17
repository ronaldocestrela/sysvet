using System.Net.Http.Json;
using Clients.Infrastructure.Http;
using Microsoft.Extensions.Http;
using SharedUI.Services;

namespace SharedUI.Http;

/// <summary>
/// Uses the unauthenticated <c>Auth</c> HTTP client to refresh expired access tokens.
/// </summary>
public sealed class AuthTokenRefresher : IAuthTokenRefresher
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuthState _authState;

    /// <summary>Creates a refresher bound to the host DI container.</summary>
    public AuthTokenRefresher(IHttpClientFactory httpClientFactory, IAuthState authState)
    {
        _httpClientFactory = httpClientFactory;
        _authState = authState;
    }

    /// <inheritdoc />
    public async Task<bool> TryRefreshAsync(CancellationToken cancellationToken = default)
    {
        var refreshToken = await _authState.GetRefreshTokenAsync();
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return false;
        }

        var client = _httpClientFactory.CreateClient("Auth");
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new RefreshTokenRequest { RefreshToken = refreshToken },
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await _authState.LogoutAsync();
            return false;
        }

        var tokens = await response.Content.ReadFromJsonAsync<AuthTokensDto>(cancellationToken);
        if (tokens is null || string.IsNullOrWhiteSpace(tokens.AccessToken))
        {
            await _authState.LogoutAsync();
            return false;
        }

        await _authState.LoginAsync(tokens.AccessToken, tokens.RefreshToken);
        return true;
    }
}
