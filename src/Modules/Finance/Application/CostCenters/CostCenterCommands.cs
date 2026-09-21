using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Finance.Application.CostCenters.Dtos;

namespace Finance.Application.CostCenters;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.FinanceRead)]
public record ListCostCentersQuery() : IQuery<IReadOnlyList<CostCenterDto>>;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.FinanceWrite)]
public record UpsertCostCenterCommand(
    Guid Id,
    string Code,
    string Name,
    bool IsActive,
    Guid IdempotencyKey = default) : IIdempotentCommand<Guid>;
