using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Entities;

namespace Core.Application.Pets.Queries;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff)]
public record GetPetByIdQuery(Guid Id) : IQuery<PetDto>;

public class PetDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public PetSpecies Species { get; set; }
    public string Breed { get; set; } = string.Empty;
    public PetSex Sex { get; set; }
    public Guid TutorId { get; set; }
    public DateOnly? BirthDate { get; set; }
}
