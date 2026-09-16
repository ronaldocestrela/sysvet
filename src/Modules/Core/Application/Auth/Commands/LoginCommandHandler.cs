using Core.Application.Auth.Dtos;
using Core.Application.Common.Interfaces;
using Core.Domain;
using MediatR;

namespace Core.Application.Auth.Commands;

/// <summary>
/// Validates credentials and returns JWT access and refresh tokens.
/// </summary>
public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthTokensDto>>
{
    private readonly IIdentityService _identityService;
    private readonly IAccessTokenIssuer _accessTokenIssuer;
    private readonly IRefreshTokenStore _refreshTokenStore;

    public LoginCommandHandler(
        IIdentityService identityService,
        IAccessTokenIssuer accessTokenIssuer,
        IRefreshTokenStore refreshTokenStore)
    {
        _identityService = identityService;
        _accessTokenIssuer = accessTokenIssuer;
        _refreshTokenStore = refreshTokenStore;
    }

    /// <inheritdoc />
    public async Task<Result<AuthTokensDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var userResult = await _identityService.ValidateCredentialsAsync(request.Email, request.Password, cancellationToken);
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
