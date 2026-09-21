using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Sales.Domain.Enums;

namespace Sales.Application.CashRegisters.Commands;

[AuthorizeRequest(AuthorizationPolicies.Cashier, Permissions.CashRegisterWrite)]
public sealed class RecordCashMovementCommand : ICommand<Guid>, IIdempotentCommand<Guid>
{
    public Guid CashRegisterId { get; set; }
    public CashMovementKind Kind { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid? MovementId { get; set; }
    public Guid IdempotencyKey { get; set; }
}
