using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Application.Users.Dtos;

namespace Core.Application.Users.Queries;

/// <summary>
/// Paginated staff user list for the current tenant.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.Admin)]
public sealed record ListUsersQuery(int Page = 1, int PageSize = 10, string? Search = null) : IQuery<PagedResult<StaffUserDto>>;
