using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;

namespace Sales.Application.CashRegisters.Commands;

[AuthorizeRequest(AuthorizationPolicies.Cashier)]
public class OpenCashRegisterCommand : ICommand<Guid>
{
    public decimal OpeningBalance { get; set; }
}
