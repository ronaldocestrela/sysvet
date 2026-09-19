using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Sales.Application.CashRegisters.Commands;

[AuthorizeRequest(AuthorizationPolicies.Cashier, Permissions.CashRegisterWrite)]
public class CloseCashRegisterCommand : ICommand<bool>, IIdempotentCommand<bool>
{
    public Guid CashRegisterId { get; set; }
    public decimal ActualClosingBalance { get; set; }
    public Guid IdempotencyKey { get; set; }
}
