using Core.Application.Auth.Dtos;
using Core.Application.Authorization;
using Core.Application.Common.Interfaces;
using Core.Domain;
using MediatR;

namespace TutorPortal.Application.Auth.Commands;

/// <summary>
/// Refreshes JWT access tokens for tutor portal users only.
/// </summary>
public sealed class RefreshTutorTokenCommandHandler : IRequestHandler<RefreshTutorTokenCommand, Result<AuthTokensDto>>
{
    private readonly IRefreshTokenStore _refreshTokenStore;
    private readonly TutorPortalAuthService _authService;

    public RefreshTutorTokenCommandHandler(IRefreshTokenStore refreshTokenStore, TutorPortalAuthService authService)
    {
        _refreshTokenStore = refreshTokenStore;
        _authService = authService;
    }

    /// <inheritdoc />
    public async Task<Result<AuthTokensDto>> Handle(RefreshTutorTokenCommand request, CancellationToken cancellationToken)
    {
        var userResult = await _refreshTokenStore.RotateAsync(request.RefreshToken, cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure<AuthTokensDto>(userResult.Error);
        }

        var user = userResult.Value;
        if (!user.Roles.Contains(ApplicationRoles.Tutor))
        {
            return Result.Failure<AuthTokensDto>(TutorPortal.Domain.ErrorCodes.Account.WrongPortal);
        }

        return await _authService.IssueTokensAsync(user, cancellationToken);
    }
}
