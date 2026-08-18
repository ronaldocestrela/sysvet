using Core.Domain;
using Core.Domain.Entities;
using MediatR;

namespace Core.Application.Pets.Queries;

public record GetPetByIdQuery(Guid Id) : IRequest<Result<PetDto>>;

public class PetDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public PetSpecies Species { get; set; }
    public string Breed { get; set; } = string.Empty;
    public PetSex Sex { get; set; }
    public Guid TutorId { get; set; }
}
