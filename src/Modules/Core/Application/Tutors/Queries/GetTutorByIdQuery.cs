using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;

namespace Core.Application.Tutors.Queries;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff)]
public record GetTutorByIdQuery(Guid Id) : IQuery<TutorDto>;

public class TutorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
