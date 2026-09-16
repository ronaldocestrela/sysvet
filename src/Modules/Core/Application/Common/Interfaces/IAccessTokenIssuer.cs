using Core.Application.Auth.Dtos;

namespace Core.Application.Common.Interfaces;

/// <summary>
/// Issues signed JWT access tokens from application user data.
/// </summary>
public interface IAccessTokenIssuer
{
    /// <summary>
    /// Creates a bearer access token including tenant and role claims.
    /// </summary>
    string IssueAccessToken(AuthenticatedUserDto user);

    /// <summary>
    /// Access token lifetime in seconds (for OAuth-style responses).
    /// </summary>
    int AccessTokenLifetimeSeconds { get; }
}
