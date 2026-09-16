using Core.Application.Common;
using Core.Domain;
using MediatR;

namespace Core.Application.AuditLogs.Queries;

/// <summary>
/// Returns paginated audit logs for the current tenant with optional filters.
/// </summary>
public sealed class ListAuditLogsQueryHandler : IRequestHandler<ListAuditLogsQuery, Result<PagedResult<AuditLogDto>>>
{
    private readonly IAuditLogRepository _auditLogRepository;

    public ListAuditLogsQueryHandler(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    /// <inheritdoc />
    public async Task<Result<PagedResult<AuditLogDto>>> Handle(ListAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var page = await _auditLogRepository.SearchAsync(
            request.Page,
            request.PageSize,
            request.EntityName,
            request.EntityId,
            request.AuditAction,
            request.OccurredFrom,
            request.OccurredTo,
            cancellationToken);

        var items = page.Items.Select(AuditLogMappings.ToDto).ToList();
        var paged = new PagedResult<AuditLogDto>(items, request.Page, request.PageSize, page.TotalCount);

        return Result.Success(paged);
    }
}
