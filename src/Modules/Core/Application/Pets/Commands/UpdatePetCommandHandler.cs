using Core.Domain;
using Core.Domain.Entities;
using MediatR;

namespace Core.Application.Pets.Commands;

public class UpdatePetCommandHandler : IRequestHandler<UpdatePetCommand, Result>
{
    private readonly IPetRepository _petRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePetCommandHandler(IPetRepository petRepository, IUnitOfWork unitOfWork)
    {
        _petRepository = petRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdatePetCommand request, CancellationToken cancellationToken)
    {
        var pet = await _petRepository.GetByIdAsync(request.Id, cancellationToken);
        if (pet == null)
            return Result.Failure(new Error("Pet.NotFound", $"O Pet com ID '{request.Id}' não foi encontrado."));

        var updateResult = pet.Update(request.Name, request.Species, request.Breed, request.Sex);
        if (updateResult.IsFailure) return updateResult;

        _petRepository.Update(pet);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
