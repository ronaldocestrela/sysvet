using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;

namespace Core.Application.Users.Commands;

/// <summary>
/// Updates staff user display name and access profile.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.Admin)]
public sealed record UpdateUserCommand(string UserId, Guid AccessProfileId, string? DisplayName) : ICommand;
