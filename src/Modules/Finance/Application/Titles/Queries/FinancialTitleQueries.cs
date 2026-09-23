using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Finance.Application.Titles.Dtos;
using Finance.Domain.Enums;

namespace Finance.Application.Titles.Queries;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.FinanceRead)]
public record GetFinancialTitleByIdQuery(Guid Id) : IQuery<FinancialTitleDto>;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.FinanceRead)]
public record ListFinancialTitlesQuery(
    TitleDirection? Direction = null,
    TitleStatus? Status = null,
    PartyKind? PartyKind = null,
    Guid? PartyId = null,
    DateOnly? DueFrom = null,
    DateOnly? DueTo = null,
    int Page = 1,
    int PageSize = Core.Application.Common.PageRequest.DefaultPageSize) : IQuery<Core.Application.Common.PagedResult<FinancialTitleDto>>;
