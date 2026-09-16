using Core.Application.Auth.Dtos;
using Core.Domain;

namespace Core.Application.Common.Interfaces;

/// <summary>
/// Persists refresh tokens as hashes with rotation and expiry.
/// </summary>
public interface IRefreshTokenStore
{
    /// <summary>
    /// Creates a new refresh token for the user and returns the plain token (once) to the client.
    /// </summary>
    Task<string> IssueAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the refresh token, revokes it, and issues a new pair for the user.
    /// </summary>
    Task<Result<AuthenticatedUserDto>> RotateAsync(string refreshToken, CancellationToken cancellationToken = default);
}
