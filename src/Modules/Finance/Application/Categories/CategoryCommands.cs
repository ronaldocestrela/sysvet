using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Finance.Application.Categories.Dtos;
using Finance.Domain.Enums;

namespace Finance.Application.Categories;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.FinanceRead)]
public record ListFinancialCategoriesQuery() : IQuery<IReadOnlyList<FinancialCategoryDto>>;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.FinanceWrite)]
public record UpsertFinancialCategoryCommand(
    Guid Id,
    string Code,
    string Name,
    CategoryDirection Direction,
    bool IsActive,
    Guid IdempotencyKey = default) : IIdempotentCommand<Guid>;
