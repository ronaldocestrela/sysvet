using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Domain.Authorization;

namespace Core.Application.Tutors.Commands;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.TutorsWrite)]
public record UpdateTutorCommand(Guid Id, string Name, string Email, string Phone, Guid IdempotencyKey = default) : IIdempotentCommand;
