using Core.Domain;
using Core.Domain.Entities;
using MediatR;

namespace Core.Application.AccessProfiles.Commands;

/// <summary>
/// Clones permissions from a source profile into a new custom profile.
/// </summary>
public sealed class CreateAccessProfileCommandHandler : IRequestHandler<CreateAccessProfileCommand, Result<Guid>>
{
    private readonly IAccessProfileRepository _accessProfileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAccessProfileCommandHandler(IAccessProfileRepository accessProfileRepository, IUnitOfWork unitOfWork)
    {
        _accessProfileRepository = accessProfileRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(CreateAccessProfileCommand request, CancellationToken cancellationToken)
    {
        var existingName = await _accessProfileRepository.GetByNameAsync(request.Name, cancellationToken);
        if (existingName is not null)
        {
            return Result.Failure<Guid>(ErrorCodes.AccessProfile.DuplicateName);
        }

        var source = await _accessProfileRepository.GetByIdAsync(request.SourceProfileId, cancellationToken);
        if (source is null)
        {
            return Result.Failure<Guid>(ErrorCodes.AccessProfile.NotFound);
        }

        var clone = source.Clone(request.Name);
        if (clone.IsFailure)
        {
            return Result.Failure<Guid>(clone.Error);
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            clone.Value.SetDescription(request.Description);
        }

        _accessProfileRepository.Add(clone.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(clone.Value.Id);
    }
}
