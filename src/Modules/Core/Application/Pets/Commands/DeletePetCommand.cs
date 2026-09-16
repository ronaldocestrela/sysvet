using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Domain.Authorization;

namespace Core.Application.Pets.Commands;

/// <summary>
/// Soft-deletes a pet record.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.PetsDelete)]
public record DeletePetCommand(Guid Id, Guid IdempotencyKey = default) : IIdempotentCommand;
