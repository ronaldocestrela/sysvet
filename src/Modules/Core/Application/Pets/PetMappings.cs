using Core.Application.Pets.Queries;
using Core.Domain.Entities;

namespace Core.Application.Pets;

/// <summary>
/// Maps domain pets to API-facing DTOs.
/// </summary>
public static class PetMappings
{
    /// <summary>
    /// Projects a pet entity to a read model DTO.
    /// </summary>
    public static PetDto ToDto(Pet pet) => new()
    {
        Id = pet.Id,
        Name = pet.Name,
        Species = pet.Species,
        Breed = pet.Breed,
        Sex = pet.Sex,
        TutorId = pet.TutorId,
        BirthDate = pet.BirthDate
    };
}
