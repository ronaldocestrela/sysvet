using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Application.Users.Dtos;

namespace Core.Application.Users.Queries;

/// <summary>
/// Loads a staff user by id within the tenant.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.Admin)]
public sealed record GetUserByIdQuery(string UserId) : IQuery<StaffUserDto>;
