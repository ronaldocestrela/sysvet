using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Sales.Application.CashRegisters.Commands;

[AuthorizeRequest(AuthorizationPolicies.Cashier, Permissions.CashRegisterWrite)]
public class OpenCashRegisterCommand : ICommand<Guid>, IIdempotentCommand<Guid>
{
    public decimal OpeningBalance { get; set; }
    public Guid IdempotencyKey { get; set; }
}
