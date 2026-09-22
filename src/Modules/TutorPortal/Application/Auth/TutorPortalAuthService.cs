using Core.Application.Auth.Dtos;
using Core.Application.Authorization;
using Core.Application.Common.Interfaces;
using Core.Domain;
using TutorPortal.Domain.Repositories;

namespace TutorPortal.Application.Auth;

/// <summary>
/// Issues JWT pairs for tutor portal users after enriching with CRM tutor linkage.
/// </summary>
public sealed class TutorPortalAuthService
{
    private readonly IAccessTokenIssuer _accessTokenIssuer;
    private readonly IRefreshTokenStore _refreshTokenStore;
    private readonly ITutorPortalAccountRepository _accountRepository;

    public TutorPortalAuthService(
        IAccessTokenIssuer accessTokenIssuer,
        IRefreshTokenStore refreshTokenStore,
        ITutorPortalAccountRepository accountRepository)
    {
        _accessTokenIssuer = accessTokenIssuer;
        _refreshTokenStore = refreshTokenStore;
        _accountRepository = accountRepository;
    }

    /// <summary>
    /// Builds tokens when the authenticated user has role Tutor and a portal account linkage.
    /// </summary>
    public async Task<Result<AuthTokensDto>> IssueTokensAsync(AuthenticatedUserDto user, CancellationToken cancellationToken)
    {
        if (!user.Roles.Contains(ApplicationRoles.Tutor))
        {
            return Result.Failure<AuthTokensDto>(TutorPortal.Domain.ErrorCodes.Account.WrongPortal);
        }

        var account = await _accountRepository.GetByUserIdAsync(user.UserId, cancellationToken);
        if (account is null)
        {
            return Result.Failure<AuthTokensDto>(TutorPortal.Domain.ErrorCodes.Account.NotFound);
        }

        var enriched = user with { TutorId = account.TutorId };
        var accessToken = _accessTokenIssuer.IssueAccessToken(enriched);
        var refreshToken = await _refreshTokenStore.IssueAsync(user.UserId, cancellationToken);
        return Result.Success(new AuthTokensDto(accessToken, refreshToken, _accessTokenIssuer.AccessTokenLifetimeSeconds));
    }
}
