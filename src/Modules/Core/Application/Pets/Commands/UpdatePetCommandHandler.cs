using Core.Domain;
using Core.Domain.Entities;
using MediatR;

namespace Core.Application.Pets.Commands;

public class UpdatePetCommandHandler : IRequestHandler<UpdatePetCommand, Result>
{
    private readonly IPetRepository _petRepository;

    public UpdatePetCommandHandler(IPetRepository petRepository)
    {
        _petRepository = petRepository;
    }

    public async Task<Result> Handle(UpdatePetCommand request, CancellationToken cancellationToken)
    {
        var pet = await _petRepository.GetByIdAsync(request.Id, cancellationToken);
        if (pet == null)
        {
            return Result.Failure(ErrorCodes.Pet.NotFound);
        }

        if (request.OccurredAt.HasValue && request.OccurredAt.Value < pet.UpdatedAt)
        {
            return Result.Success();
        }

        var updateResult = pet.Update(request.Name, request.Species, request.Breed, request.Sex);
        if (updateResult.IsFailure) return updateResult;

        if (request.OccurredAt.HasValue)
        {
            pet.UpdatedAt = request.OccurredAt.Value;
        }

        _petRepository.Update(pet);

        return Result.Success();
    }
}
