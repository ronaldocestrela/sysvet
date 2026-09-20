using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Sales.Domain.Enums;

namespace Sales.Application.Prepaid;

[AuthorizeRequest(AuthorizationPolicies.Cashier, Permissions.SalesRead)]
public sealed record ListPrepaidBalancesQuery(
    Guid? TutorId,
    Guid? PetId,
    ServiceCode? ServiceCode) : IQuery<IReadOnlyList<PrepaidBalanceDto>>;

[AuthorizeRequest(AuthorizationPolicies.Cashier, Permissions.SalesWrite)]
public sealed class ConsumePrepaidPackageUseCommand : ICommand<bool>, IIdempotentCommand<bool>
{
    public Guid UsageId { get; set; }
    public Guid PetId { get; set; }
    public ServiceCode ServiceCode { get; set; }
    public string? AttendanceRef { get; set; }
    public Guid IdempotencyKey { get; set; }
}

public sealed record PrepaidBalanceDto(
    Guid Id,
    Guid TutorId,
    Guid PetId,
    ServiceCode ServiceCode,
    int RemainingUses,
    int PurchasedUses);
