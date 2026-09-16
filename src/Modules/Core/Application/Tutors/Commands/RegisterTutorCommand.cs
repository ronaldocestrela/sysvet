using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;

namespace Core.Application.Tutors.Commands;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff)]
public record RegisterTutorCommand(Guid Id, string Name, string Email, string Cpf, string Phone, Guid IdempotencyKey = default) : IIdempotentCommand<Guid>;
