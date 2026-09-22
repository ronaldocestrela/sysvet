using Core.Application.Auth.Dtos;
using Core.Application.Authorization;
using Core.Application.Common.Interfaces;
using Core.Domain;
using MediatR;

namespace TutorPortal.Application.Auth.Commands;

/// <summary>
/// Authenticates tutor portal users and rejects clinic staff credentials.
/// </summary>
public sealed class TutorLoginCommandHandler : IRequestHandler<TutorLoginCommand, Result<AuthTokensDto>>
{
    private readonly IIdentityService _identityService;
    private readonly TutorPortalAuthService _authService;

    public TutorLoginCommandHandler(IIdentityService identityService, TutorPortalAuthService authService)
    {
        _identityService = identityService;
        _authService = authService;
    }

    /// <inheritdoc />
    public async Task<Result<AuthTokensDto>> Handle(TutorLoginCommand request, CancellationToken cancellationToken)
    {
        var userResult = await _identityService.ValidateCredentialsAsync(request.Email, request.Password, cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure<AuthTokensDto>(userResult.Error);
        }

        var user = userResult.Value;
        if (!user.Roles.Contains(ApplicationRoles.Tutor))
        {
            return Result.Failure<AuthTokensDto>(TutorPortal.Domain.ErrorCodes.Account.WrongPortal);
        }

        if (user.Roles.Any(r => r != ApplicationRoles.Tutor))
        {
            return Result.Failure<AuthTokensDto>(TutorPortal.Domain.ErrorCodes.Account.WrongPortal);
        }

        return await _authService.IssueTokensAsync(user, cancellationToken);
    }
}
