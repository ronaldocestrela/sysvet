using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Finance.Domain.Enums;

namespace Finance.Application.Titles.Commands;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.FinanceWrite)]
public record CreateManualFinancialTitleCommand(
    Guid Id,
    TitleDirection Direction,
    Guid CategoryId,
    PartyKind PartyKind,
    Guid? PartyId,
    decimal Amount,
    DateOnly IssueDate,
    DateOnly DueDate,
    string Description,
    Guid? CostCenterId = null,
    Guid IdempotencyKey = default) : IIdempotentCommand<Guid>;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.FinanceWrite)]
public record SettleFinancialTitleCommand(
    Guid TitleId,
    decimal Amount,
    string Method,
    DateTimeOffset? PaidAt = null,
    Guid IdempotencyKey = default) : IIdempotentCommand;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.FinanceWrite)]
public record CancelFinancialTitleCommand(
    Guid TitleId,
    Guid IdempotencyKey = default) : IIdempotentCommand;
