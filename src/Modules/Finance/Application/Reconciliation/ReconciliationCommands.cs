using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Finance.Application.Reconciliation.Dtos;

namespace Finance.Application.Reconciliation;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.FinanceWrite)]
public sealed class ImportCardStatementCommand : ICommand<Guid>, IIdempotentCommand<Guid>
{
    public string Reference { get; set; } = string.Empty;
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
    public IReadOnlyList<ImportCardStatementLineRequest> Lines { get; set; } = Array.Empty<ImportCardStatementLineRequest>();
    public Guid IdempotencyKey { get; set; }
}

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.FinanceRead)]
public sealed class ListCardReconciliationsQuery : IQuery<IReadOnlyList<CardReconciliationBatchSummaryDto>>;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.FinanceRead)]
public sealed class GetCardReconciliationByIdQuery(Guid batchId) : IQuery<CardReconciliationBatchDetailDto?>
{
    public Guid BatchId { get; } = batchId;
}

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.FinanceRead)]
public sealed class GetUnmatchedCardSettlementsQuery : IQuery<IReadOnlyList<UnmatchedCardSettlementDto>>;
