using SharedUI.Services;
using System.Net.Http.Headers;

namespace MauiApp.Services;

public class AuthHandler : DelegatingHandler
{
    private readonly IAuthState _authState;

    public AuthHandler(IAuthState authState)
    {
        _authState = authState;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _authState.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
