using SharedUI.Services;

namespace Clients.Tests.Fakes;

/// <summary>
/// Test double for <see cref="IAuthState"/> that raises <see cref="SessionChanged"/> on mutations.
/// </summary>
public sealed class FakeAuthState : IAuthState
{
    private readonly IReadOnlyList<string> _menus;

    public FakeAuthState(IReadOnlyList<string>? menus = null, bool isAuthenticated = true)
    {
        _menus = menus ?? [];
        IsAuthenticated = isAuthenticated;
    }

    public bool IsAuthenticated { get; private set; }

    public IReadOnlyList<string> Menus => _menus;

    public event EventHandler? SessionChanged;

    public Task InitializeAsync() => Task.CompletedTask;

    public Task<string?> GetTokenAsync() => Task.FromResult<string?>("token");

    public Task<string?> GetRefreshTokenAsync() => Task.FromResult<string?>("refresh");

    public Task LoginAsync(string accessToken, string refreshToken)
    {
        IsAuthenticated = true;
        SessionChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public Task SetMenusAsync(IReadOnlyList<string> menus)
    {
        SessionChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public Task LogoutAsync()
    {
        IsAuthenticated = false;
        SessionChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }
}
