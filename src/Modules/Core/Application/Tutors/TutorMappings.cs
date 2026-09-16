using Core.Application.Tutors.Queries;
using Core.Domain.Entities;

namespace Core.Application.Tutors;

/// <summary>
/// Maps domain tutors to API-facing DTOs without exposing entity types from handlers.
/// </summary>
public static class TutorMappings
{
    /// <summary>
    /// Projects a tutor aggregate to a read model DTO.
    /// </summary>
    public static TutorDto ToDto(Tutor tutor) => new()
    {
        Id = tutor.Id,
        Name = tutor.Name,
        Email = tutor.Email.Address,
        Cpf = tutor.Cpf.Number,
        Phone = tutor.Phone.Number
    };
}
