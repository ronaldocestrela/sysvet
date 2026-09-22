using Core.Application.Common.Interfaces;
using Core.Domain;
using TutorPortal.Domain.Repositories;

namespace TutorPortal.Application.Auth;

/// <summary>
/// Resolves the CRM tutor identifier for the authenticated tutor portal user.
/// </summary>
public sealed class TutorPortalUserResolver
{
    private readonly ICurrentUser _currentUser;
    private readonly ITutorPortalAccountRepository _accountRepository;

    /// <summary>
    /// Creates the resolver with JWT and portal account dependencies.
    /// </summary>
    public TutorPortalUserResolver(ICurrentUser currentUser, ITutorPortalAccountRepository accountRepository)
    {
        _currentUser = currentUser;
        _accountRepository = accountRepository;
    }

    /// <summary>
    /// Returns the linked CRM tutor id or a failure when the caller is not a portal tutor.
    /// </summary>
    public async Task<Result<Guid>> ResolveTutorIdAsync(CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || string.IsNullOrEmpty(_currentUser.UserId))
        {
            return Result.Failure<Guid>(ErrorCodes.Authorization.Unauthorized);
        }

        var tutorId = _currentUser.TutorId;
        if (tutorId is null || tutorId == Guid.Empty)
        {
            var account = await _accountRepository.GetByUserIdAsync(_currentUser.UserId, cancellationToken);
            tutorId = account?.TutorId;
        }

        if (tutorId is null || tutorId == Guid.Empty)
        {
            return Result.Failure<Guid>(TutorPortal.Domain.ErrorCodes.Account.NotFound);
        }

        return Result.Success(tutorId.Value);
    }
}
