using Core.Application.Authorization;
using Core.Application.AccessProfiles.Dtos;
using Core.Application.Behaviors;
using Core.Application.Messaging;

namespace Core.Application.AccessProfiles.Queries;

/// <summary>
/// Loads an access profile with its permission matrix.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.Admin)]
public sealed record GetAccessProfileByIdQuery(Guid Id) : IQuery<AccessProfileDto>;
