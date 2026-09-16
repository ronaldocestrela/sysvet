using Core.Application.AuditLogs.Queries;
using Core.Application.Authorization;
using Core.Application.Common;
using MediatR;

namespace API.Extensions;

/// <summary>
/// Read-only audit log endpoints under <c>/api/v1/audit-logs</c>.
/// </summary>
public static class AuditLogEndpointsExtensions
{
    /// <summary>
    /// Maps paginated audit log queries for tenant administrators.
    /// </summary>
    public static void MapAuditLogEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/audit-logs")
            .RequireAuthorization(AuthorizationPolicies.Admin)
            .WithTags("Core", "AuditLogs");

        group.MapGet("/", async (
            IMediator mediator,
            int page = 1,
            int pageSize = 20,
            string? entityName = null,
            Guid? entityId = null,
            string? auditAction = null) =>
        {
            var query = new ListAuditLogsQuery(page, pageSize, entityName, entityId, auditAction);
            return (await mediator.Send(query)).ToHttpResult();
        })
            .WithName("ListAuditLogs")
            .WithSummary("List audit logs")
            .WithDescription("Paginated append-only audit trail for the current tenant.")
            .Produces<PagedResult<AuditLogDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
