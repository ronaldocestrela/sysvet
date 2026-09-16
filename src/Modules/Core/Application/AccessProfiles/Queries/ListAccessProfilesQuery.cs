using Core.Application.Authorization;
using Core.Application.AccessProfiles.Dtos;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;

namespace Core.Application.AccessProfiles.Queries;

/// <summary>
/// Paginated access profile list for the tenant.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.Admin)]
public sealed record ListAccessProfilesQuery(int Page = 1, int PageSize = 20, string? NameFilter = null) : IQuery<PagedResult<AccessProfileDto>>;
