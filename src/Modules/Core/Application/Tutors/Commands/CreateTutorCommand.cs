using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Domain.Authorization;

namespace Core.Application.Tutors.Commands;

/// <summary>
/// Creates a new tutor in the CRM.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.TutorsWrite)]
public record CreateTutorCommand(Guid Id, string Name, string Email, string Cpf, string Phone, Guid IdempotencyKey = default) : IIdempotentCommand<Guid>;
