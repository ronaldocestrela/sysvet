using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Sales.Domain.Enums;

namespace Sales.Application.Commissions;

[AuthorizeRequest(AuthorizationPolicies.Admin)]
public sealed record UpsertCommissionRuleCommand(
    CommissionRole Role,
    CommissionAppliesTo AppliesTo,
    decimal RatePercent) : ICommand<Guid>;

[AuthorizeRequest(AuthorizationPolicies.Cashier, Core.Domain.Authorization.Permissions.SalesRead)]
public sealed record ListCommissionRulesQuery : IQuery<IReadOnlyList<CommissionRuleDto>>;

[AuthorizeRequest(AuthorizationPolicies.Cashier, Core.Domain.Authorization.Permissions.SalesRead)]
public sealed record ListCommissionAccrualsQuery(Guid? OrderId) : IQuery<IReadOnlyList<CommissionAccrualDto>>;

public sealed record CommissionRuleDto(Guid Id, CommissionRole Role, CommissionAppliesTo AppliesTo, decimal RatePercent);

public sealed record CommissionAccrualDto(
    Guid Id,
    Guid OrderId,
    Guid OrderItemId,
    Guid PayeeUserId,
    CommissionRole Role,
    decimal RatePercent,
    decimal BaseAmount,
    decimal CommissionAmount,
    CommissionAccrualStatus Status);
