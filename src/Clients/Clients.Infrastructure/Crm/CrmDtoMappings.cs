using Clients.Infrastructure.Http;
using Core.Domain.Entities;

namespace Clients.Infrastructure.Crm;

/// <summary>
/// Maps Core CRM entities to HTTP DTOs used by SharedUI (mirrors API contract).
/// </summary>
public static class CrmDtoMappings
{
    /// <summary>Projects a tutor to a list/detail DTO.</summary>
    public static TutorDto ToDto(Tutor tutor) => new()
    {
        Id = tutor.Id,
        Name = tutor.Name,
        Email = tutor.Email.Address,
        Cpf = tutor.Cpf.Number,
        Phone = tutor.Phone.Number
    };

    /// <summary>Projects a pet to a list/detail DTO.</summary>
    public static PetDto ToDto(Pet pet) => new()
    {
        Id = pet.Id,
        Name = pet.Name,
        Species = (PetSpeciesDto)(int)pet.Species,
        Breed = pet.Breed,
        Sex = (PetSexDto)(int)pet.Sex,
        TutorId = pet.TutorId
    };

    /// <summary>Converts API species enum to domain.</summary>
    public static PetSpecies ToDomainSpecies(PetSpeciesDto species) => (PetSpecies)(int)species;

    /// <summary>Converts API sex enum to domain.</summary>
    public static PetSex ToDomainSex(PetSexDto sex) => (PetSex)(int)sex;
}
