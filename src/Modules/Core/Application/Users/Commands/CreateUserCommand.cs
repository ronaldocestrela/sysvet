using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;

namespace Core.Application.Users.Commands;

/// <summary>
/// Creates a staff user in the current tenant.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.Admin)]
public sealed record CreateUserCommand(
    string Email,
    string Password,
    Guid AccessProfileId,
    string? DisplayName = null) : ICommand<string>;
