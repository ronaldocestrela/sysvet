using Core.Domain;
using MediatR;

namespace Core.Application.Pets.Queries;

public class ListPetsQueryHandler : IRequestHandler<ListPetsQuery, Result<IEnumerable<PetDto>>>
{
    private readonly IPetRepository _petRepository;

    public ListPetsQueryHandler(IPetRepository petRepository)
    {
        _petRepository = petRepository;
    }

    public async Task<Result<IEnumerable<PetDto>>> Handle(ListPetsQuery request, CancellationToken cancellationToken)
    {
        var allPets = await _petRepository.GetAllAsync(cancellationToken);

        var filtered = allPets.AsEnumerable();

        if (request.TutorId.HasValue && request.TutorId.Value != Guid.Empty)
        {
            filtered = filtered.Where(p => p.TutorId == request.TutorId.Value);
        }

        var paged = filtered
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PetDto
            {
                Id = p.Id,
                Name = p.Name,
                Species = p.Species,
                Breed = p.Breed,
                Sex = p.Sex,
                TutorId = p.TutorId
            })
            .ToList();

        return Result.Success<IEnumerable<PetDto>>(paged);
    }
}
