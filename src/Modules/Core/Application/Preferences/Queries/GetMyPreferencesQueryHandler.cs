using Core.Application.Common.Interfaces;
using Core.Application.Preferences.Dtos;
using Core.Domain;
using MediatR;

namespace Core.Application.Preferences.Queries;

/// <summary>
/// Returns stored preferences or empty defaults.
/// </summary>
public sealed class GetMyPreferencesQueryHandler : IRequestHandler<GetMyPreferencesQuery, Result<UserPreferenceDto>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IUserPreferenceRepository _userPreferenceRepository;

    public GetMyPreferencesQueryHandler(ICurrentUser currentUser, IUserPreferenceRepository userPreferenceRepository)
    {
        _currentUser = currentUser;
        _userPreferenceRepository = userPreferenceRepository;
    }

    /// <inheritdoc />
    public async Task<Result<UserPreferenceDto>> Handle(GetMyPreferencesQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || string.IsNullOrEmpty(_currentUser.UserId))
        {
            return Result.Failure<UserPreferenceDto>(ErrorCodes.Authorization.Unauthorized);
        }

        var existing = await _userPreferenceRepository.GetByUserIdAsync(_currentUser.UserId, cancellationToken);
        if (existing is null)
        {
            return Result.Success(new UserPreferenceDto("{}", "{}", "{}"));
        }

        return Result.Success(new UserPreferenceDto(
            existing.ShortcutsJson,
            existing.SavedFiltersJson,
            existing.ColumnLayoutsJson));
    }
}
