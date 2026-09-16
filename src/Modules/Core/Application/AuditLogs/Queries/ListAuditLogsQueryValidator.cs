using FluentValidation;

namespace Core.Application.AuditLogs.Queries;

/// <summary>
/// Validates paging bounds for audit log listing.
/// </summary>
public sealed class ListAuditLogsQueryValidator : AbstractValidator<ListAuditLogsQuery>
{
    public ListAuditLogsQueryValidator()
    {
        RuleFor(q => q.Page).GreaterThanOrEqualTo(1);
        RuleFor(q => q.PageSize).InclusiveBetween(1, 100);
    }
}
