using Core.Domain;
using MediatR;

namespace Core.Application.Pets.Commands;

/// <summary>
/// Soft-deletes a pet entity.
/// </summary>
public class DeletePetCommandHandler : IRequestHandler<DeletePetCommand, Result>
{
    private readonly IPetRepository _petRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePetCommandHandler"/> class.
    /// </summary>
    public DeletePetCommandHandler(IPetRepository petRepository)
    {
        _petRepository = petRepository;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(DeletePetCommand request, CancellationToken cancellationToken)
    {
        var pet = await _petRepository.GetByIdAsync(request.Id, cancellationToken);
        if (pet is null)
        {
            return Result.Failure(ErrorCodes.Pet.NotFound);
        }

        pet.SoftDelete();
        _petRepository.Update(pet);

        return Result.Success();
    }
}
