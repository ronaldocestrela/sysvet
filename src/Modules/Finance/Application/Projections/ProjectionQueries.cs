using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Finance.Application.Projections.Dtos;
using Finance.Domain.Enums;

namespace Finance.Application.Projections;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.FinanceRead)]
public record GetBalanceProjectionQuery(DateOnly From, DateOnly To) : IQuery<BalanceProjectionDto>;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.FinanceRead)]
public record GetPartyLedgerQuery(PartyKind PartyKind, Guid PartyId) : IQuery<IReadOnlyList<PartyLedgerEntryDto>>;
