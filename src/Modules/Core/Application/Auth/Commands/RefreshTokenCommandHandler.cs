using Core.Application.Auth.Dtos;
using Core.Application.Common.Interfaces;
using Core.Domain;
using MediatR;

namespace Core.Application.Auth.Commands;

/// <summary>
/// Rotates refresh tokens and issues a new access token.
/// </summary>
public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthTokensDto>>
{
    private readonly IRefreshTokenStore _refreshTokenStore;
    private readonly IAccessTokenIssuer _accessTokenIssuer;

    public RefreshTokenCommandHandler(IRefreshTokenStore refreshTokenStore, IAccessTokenIssuer accessTokenIssuer)
    {
        _refreshTokenStore = refreshTokenStore;
        _accessTokenIssuer = accessTokenIssuer;
    }

    /// <inheritdoc />
    public async Task<Result<AuthTokensDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var userResult = await _refreshTokenStore.RotateAsync(request.RefreshToken, cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure<AuthTokensDto>(userResult.Error);
        }

        var user = userResult.Value;
        var accessToken = _accessTokenIssuer.IssueAccessToken(user);
        var refreshToken = await _refreshTokenStore.IssueAsync(user.UserId, cancellationToken);

        return Result.Success(new AuthTokensDto(accessToken, refreshToken, _accessTokenIssuer.AccessTokenLifetimeSeconds));
    }
}
