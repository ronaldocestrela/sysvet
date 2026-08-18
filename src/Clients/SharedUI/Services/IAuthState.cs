namespace SharedUI.Services;

public interface IAuthState
{
    bool IsAuthenticated { get; }
    Task<string?> GetTokenAsync();
    Task LoginAsync(string token);
    Task LogoutAsync();
}
