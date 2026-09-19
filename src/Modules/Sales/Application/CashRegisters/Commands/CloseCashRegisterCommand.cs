using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Sales.Application.CashRegisters.Commands;

[AuthorizeRequest(AuthorizationPolicies.Cashier, Permissions.CashRegisterWrite)]
public class CloseCashRegisterCommand : ICommand<bool>
{
    public Guid CashRegisterId { get; set; }
    public decimal ActualClosingBalance { get; set; }
}
