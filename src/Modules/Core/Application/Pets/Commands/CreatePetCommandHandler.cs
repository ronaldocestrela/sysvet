using Core.Domain;
using Core.Domain.Entities;
using MediatR;

namespace Core.Application.Pets.Commands;

/// <summary>
/// Creates a pet linked to an active tutor.
/// </summary>
public class CreatePetCommandHandler : IRequestHandler<CreatePetCommand, Result<Guid>>
{
    private readonly IPetRepository _petRepository;
    private readonly ITutorRepository _tutorRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePetCommandHandler"/> class.
    /// </summary>
    public CreatePetCommandHandler(IPetRepository petRepository, ITutorRepository tutorRepository)
    {
        _petRepository = petRepository;
        _tutorRepository = tutorRepository;
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(CreatePetCommand request, CancellationToken cancellationToken)
    {
        var existingById = await _petRepository.GetByIdAsync(request.Id, cancellationToken);
        if (existingById is not null)
        {
            return Result.Success(existingById.Id);
        }

        var tutor = await _tutorRepository.GetByIdAsync(request.TutorId, cancellationToken);
        if (tutor is null)
        {
            return Result.Failure<Guid>(ErrorCodes.Pet.TutorNotFound);
        }

        if (!tutor.IsActive)
        {
            return Result.Failure<Guid>(ErrorCodes.Pet.TutorInactive);
        }

        var petResult = Pet.Create(request.Name, request.Species, request.Breed, request.Sex, request.TutorId, request.Id);
        if (petResult.IsFailure) return Result.Failure<Guid>(petResult.Error);

        var addResult = tutor.AddPet(petResult.Value);
        if (addResult.IsFailure) return Result.Failure<Guid>(addResult.Error);

        _petRepository.Add(petResult.Value);

        return Result.Success(petResult.Value.Id);
    }
}
