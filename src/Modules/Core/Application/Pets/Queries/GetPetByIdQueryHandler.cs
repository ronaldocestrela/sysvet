using Core.Domain;
using MediatR;

namespace Core.Application.Pets.Queries;

public class GetPetByIdQueryHandler : IRequestHandler<GetPetByIdQuery, Result<PetDto>>
{
    private readonly IPetRepository _petRepository;

    public GetPetByIdQueryHandler(IPetRepository petRepository)
    {
        _petRepository = petRepository;
    }

    public async Task<Result<PetDto>> Handle(GetPetByIdQuery request, CancellationToken cancellationToken)
    {
        var pet = await _petRepository.GetByIdAsync(request.Id, cancellationToken);

        if (pet == null)
        {
            return Result.Failure<PetDto>(new Error("Pet.NotFound", $"O Pet com ID '{request.Id}' não foi encontrado."));
        }

        var dto = new PetDto
        {
            Id = pet.Id,
            Name = pet.Name,
            Species = pet.Species,
            Breed = pet.Breed,
            Sex = pet.Sex,
            TutorId = pet.TutorId
        };

        return Result.Success(dto);
    }
}
