namespace Core.Application.Auth.Dtos;

/// <summary>
/// Access and refresh tokens returned after successful authentication or refresh.
/// </summary>
public sealed record AuthTokensDto(string AccessToken, string RefreshToken, int ExpiresInSeconds);
