using Core.Domain;
using Core.Domain.Entities;
using MediatR;

namespace Core.Application.Pets.Commands;

public class CreatePetCommandHandler : IRequestHandler<CreatePetCommand, Result<Guid>>
{
    private readonly IPetRepository _petRepository;
    private readonly ITutorRepository _tutorRepository;

    public CreatePetCommandHandler(IPetRepository petRepository, ITutorRepository tutorRepository)
    {
        _petRepository = petRepository;
        _tutorRepository = tutorRepository;
    }

    public async Task<Result<Guid>> Handle(CreatePetCommand request, CancellationToken cancellationToken)
    {
        var tutor = await _tutorRepository.GetByIdAsync(request.TutorId, cancellationToken);
        if (tutor == null)
        {
            return Result.Failure<Guid>(ErrorCodes.Pet.TutorNotFound);
        }

        var petResult = Pet.Create(request.Name, request.Species, request.Breed, request.Sex, request.TutorId);
        if (petResult.IsFailure) return Result.Failure<Guid>(petResult.Error);

        _petRepository.Add(petResult.Value);

        return Result.Success(petResult.Value.Id);
    }
}
