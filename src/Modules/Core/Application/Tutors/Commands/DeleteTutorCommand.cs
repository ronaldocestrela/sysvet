using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Domain.Authorization;

namespace Core.Application.Tutors.Commands;

/// <summary>
/// Soft-deletes a tutor and their pets.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.TutorsDelete)]
public record DeleteTutorCommand(Guid Id, Guid IdempotencyKey = default) : IIdempotentCommand;
