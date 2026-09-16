using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;

namespace Core.Application.AccessProfiles.Commands;

/// <summary>
/// Deletes a custom access profile when unused.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.Admin)]
public sealed record DeleteAccessProfileCommand(Guid Id) : ICommand;
