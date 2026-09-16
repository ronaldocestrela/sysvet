using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;

namespace Core.Application.Users.Commands;

/// <summary>
/// Resets a staff user's password.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.Admin)]
public sealed record ResetUserPasswordCommand(string UserId, string NewPassword) : ICommand;
