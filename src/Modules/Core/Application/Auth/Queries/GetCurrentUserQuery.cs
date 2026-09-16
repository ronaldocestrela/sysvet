using Core.Application.Auth.Dtos;
using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;

namespace Core.Application.Auth.Queries;

/// <summary>
/// Returns the profile of the authenticated caller from JWT claims.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.Authenticated)]
public sealed record GetCurrentUserQuery : IQuery<CurrentUserDto>;
