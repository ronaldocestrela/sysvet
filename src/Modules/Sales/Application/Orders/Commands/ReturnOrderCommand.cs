using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Sales.Application.Orders.Commands;

[AuthorizeRequest(AuthorizationPolicies.Cashier, Permissions.SalesWrite)]
public sealed class ReturnOrderCommand : ICommand<Guid>, IIdempotentCommand<Guid>
{
    public Guid OrderId { get; set; }
    public Guid ReturnId { get; set; }
    public List<ReturnOrderLineDto> Lines { get; set; } = new();
    public Guid IdempotencyKey { get; set; }
}

public sealed class ReturnOrderLineDto
{
    public Guid OrderItemId { get; set; }
    public decimal Quantity { get; set; }
}
