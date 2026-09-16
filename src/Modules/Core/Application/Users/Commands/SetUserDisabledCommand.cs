using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;

namespace Core.Application.Users.Commands;

/// <summary>
/// Enables or disables a staff user account.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.Admin)]
public sealed record SetUserDisabledCommand(string UserId, bool Disabled) : ICommand;
