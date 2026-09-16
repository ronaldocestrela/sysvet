using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Core.Application.AuditLogs.Queries;

/// <summary>
/// Paginated audit log listing for administrators.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.Admin, Permissions.AuditRead)]
public record ListAuditLogsQuery(
    int Page = 1,
    int PageSize = 20,
    string? EntityName = null,
    Guid? EntityId = null,
    string? AuditAction = null,
    DateTimeOffset? OccurredFrom = null,
    DateTimeOffset? OccurredTo = null) : IQuery<PagedResult<AuditLogDto>>;
