using Core.Domain;
using Core.Domain.Entities;
using MediatR;

namespace Core.Application.Tutors.Queries;

public record GetTutorByIdQuery(Guid Id) : IRequest<Result<TutorDto>>;

public class TutorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
