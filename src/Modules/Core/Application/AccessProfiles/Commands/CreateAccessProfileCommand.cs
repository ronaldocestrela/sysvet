using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;

namespace Core.Application.AccessProfiles.Commands;

/// <summary>
/// Creates a custom profile by cloning an existing profile's permissions.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.Admin)]
public sealed record CreateAccessProfileCommand(string Name, Guid SourceProfileId, string? Description = null) : ICommand<Guid>;
