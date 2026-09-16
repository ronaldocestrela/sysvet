using Core.Application.Common.Interfaces;
using Core.Domain;
using MediatR;

namespace Core.Application.AccessProfiles.Commands;

/// <summary>
/// Removes custom profiles that are not assigned to users.
/// </summary>
public sealed class DeleteAccessProfileCommandHandler : IRequestHandler<DeleteAccessProfileCommand, Result>
{
    private readonly IAccessProfileRepository _accessProfileRepository;
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAccessProfileCommandHandler(
        IAccessProfileRepository accessProfileRepository,
        IIdentityService identityService,
        IUnitOfWork unitOfWork)
    {
        _accessProfileRepository = accessProfileRepository;
        _identityService = identityService;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(DeleteAccessProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _accessProfileRepository.GetByIdAsync(request.Id, cancellationToken);
        if (profile is null)
        {
            return Result.Failure(ErrorCodes.AccessProfile.NotFound);
        }

        var canDelete = profile.MarkForDeletion();
        if (canDelete.IsFailure)
        {
            return canDelete;
        }

        var inUse = await _identityService.CountUsersWithProfileAsync(request.Id, cancellationToken);
        if (inUse > 0)
        {
            return Result.Failure(ErrorCodes.AccessProfile.InUse);
        }

        _accessProfileRepository.Remove(profile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
