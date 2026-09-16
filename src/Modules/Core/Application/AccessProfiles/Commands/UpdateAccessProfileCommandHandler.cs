using Core.Domain;
using MediatR;

namespace Core.Application.AccessProfiles.Commands;

/// <summary>
/// Applies permission matrix changes and optional rename for custom profiles.
/// </summary>
public sealed class UpdateAccessProfileCommandHandler : IRequestHandler<UpdateAccessProfileCommand, Result>
{
    private readonly IAccessProfileRepository _accessProfileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAccessProfileCommandHandler(IAccessProfileRepository accessProfileRepository, IUnitOfWork unitOfWork)
    {
        _accessProfileRepository = accessProfileRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(UpdateAccessProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _accessProfileRepository.GetByIdAsync(request.Id, cancellationToken);
        if (profile is null)
        {
            return Result.Failure(ErrorCodes.AccessProfile.NotFound);
        }

        if (!string.IsNullOrWhiteSpace(request.Name) && !string.Equals(request.Name, profile.Name, StringComparison.Ordinal))
        {
            var duplicate = await _accessProfileRepository.GetByNameAsync(request.Name, cancellationToken);
            if (duplicate is not null && duplicate.Id != profile.Id)
            {
                return Result.Failure(ErrorCodes.AccessProfile.DuplicateName);
            }

            var rename = profile.Rename(request.Name);
            if (rename.IsFailure)
            {
                return rename;
            }
        }

        if (request.Description is not null)
        {
            profile.SetDescription(request.Description);
        }

        var setPermissions = profile.SetPermissions(request.PermissionCodes);
        if (setPermissions.IsFailure)
        {
            return setPermissions;
        }

        _accessProfileRepository.Update(profile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
