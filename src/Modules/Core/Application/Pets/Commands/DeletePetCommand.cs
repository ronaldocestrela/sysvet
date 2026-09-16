using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;

namespace Core.Application.Pets.Commands;

/// <summary>
/// Soft-deletes a pet record.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff)]
public record DeletePetCommand(Guid Id, Guid IdempotencyKey = default) : IIdempotentCommand;
