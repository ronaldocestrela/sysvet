using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;

namespace Core.Application.Tutors.Commands;

/// <summary>
/// Soft-deletes a tutor and their pets.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff)]
public record DeleteTutorCommand(Guid Id, Guid IdempotencyKey = default) : IIdempotentCommand;
