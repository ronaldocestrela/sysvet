using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Sales.Application.CashRegisters.Dtos;

namespace Sales.Application.CashRegisters.Queries;

[AuthorizeRequest(AuthorizationPolicies.Cashier, Permissions.CashRegisterRead)]
public sealed class GetOpenCashRegisterQuery : IQuery<OpenCashRegisterDto?>;

[AuthorizeRequest(AuthorizationPolicies.Cashier, Permissions.CashRegisterRead)]
public sealed class GetCashRegisterByIdQuery(Guid cashRegisterId) : IQuery<CashRegisterDetailDto?>
{
    public Guid CashRegisterId { get; } = cashRegisterId;
}
