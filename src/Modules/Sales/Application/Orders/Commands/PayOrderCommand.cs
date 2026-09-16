using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;

namespace Sales.Application.Orders.Commands;

[AuthorizeRequest(AuthorizationPolicies.Cashier)]
public class PayOrderCommand : ICommand<bool>
{
    public Guid OrderId { get; set; }
}
