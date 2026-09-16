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
            return Result.Failure<PetDto>(ErrorCodes.Pet.NotFound);
        }

        return Result.Success(PetMappings.ToDto(pet));
    }
}
