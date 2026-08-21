using SharedUI.Services;

namespace MauiApp.Services;

public class MauiAuthState : IAuthState
{
    private string? _token;

    public bool IsAuthenticated => !string.IsNullOrEmpty(_token);

    public Task<string?> GetTokenAsync()
    {
        // For MAUI we would use SecureStorage.GetAsync("jwt_token")
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
