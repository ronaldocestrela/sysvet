using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;

namespace Core.Application.AccessProfiles.Queries;

/// <summary>
/// Returns the static permission catalog for profile matrix UI.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.Admin)]
public sealed record ListPermissionsQuery : IQuery<IReadOnlyList<string>>;
