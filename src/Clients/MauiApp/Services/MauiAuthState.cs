using SharedUI.Services;

namespace MauiApp.Services;

public class MauiAuthState : IAuthState
{
    private string? _token;

    public bool IsAuthenticated => !string.IsNullOrEmpty(_token);

    public async Task<string?> GetTokenAsync()
    {
        if (string.IsNullOrEmpty(_token))
        {
            _token = await SecureStorage.Default.GetAsync("jwt_token");
        }
        return _token;
    }

    public async Task LoginAsync(string token)
    {
        _token = token;
        await SecureStorage.Default.SetAsync("jwt_token", token);
    }

    public Task LogoutAsync()
    {
        _token = null;
        SecureStorage.Default.Remove("jwt_token");
        return Task.CompletedTask;
    }
}
