using Core.Application.Common.Interfaces;
using Core.Domain;
using Core.Domain.Entities;
using MediatR;

namespace Core.Application.Preferences.Commands;

/// <summary>
/// Creates or updates preference rows for the current user.
/// </summary>
public sealed class UpsertMyPreferencesCommandHandler : IRequestHandler<UpsertMyPreferencesCommand, Result>
{
    private readonly ICurrentUser _currentUser;
    private readonly IUserPreferenceRepository _userPreferenceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpsertMyPreferencesCommandHandler(
        ICurrentUser currentUser,
        IUserPreferenceRepository userPreferenceRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _userPreferenceRepository = userPreferenceRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(UpsertMyPreferencesCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || string.IsNullOrEmpty(_currentUser.UserId))
        {
            return Result.Failure(ErrorCodes.Authorization.Unauthorized);
        }

        var existing = await _userPreferenceRepository.GetByUserIdAsync(_currentUser.UserId, cancellationToken);
        var isNew = existing is null;
        if (isNew)
        {
            var created = UserPreference.Create(_currentUser.UserId);
            if (created.IsFailure)
            {
                return Result.Failure(created.Error);
            }

            existing = created.Value;
            _userPreferenceRepository.Add(existing);
        }

        var update = existing!.Update(request.ShortcutsJson, request.SavedFiltersJson, request.ColumnLayoutsJson);
        if (update.IsFailure)
        {
            return update;
        }

        if (!isNew)
        {
            _userPreferenceRepository.Update(existing);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
