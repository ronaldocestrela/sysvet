using SharedUI.Services;

namespace BlazorWeb.Services;

public class WebAuthState : IAuthState
{
    private string? _token;

    public bool IsAuthenticated => !string.IsNullOrEmpty(_token);

    public Task<string?> GetTokenAsync()
    {
        return Task.FromResult(_token);
    }

    public Task LoginAsync(string token)
    {
        _token = token;
        return Task.CompletedTask;
    }

    public Task LogoutAsync()
    {
        _token = null;
        return Task.CompletedTask;
    }
}
