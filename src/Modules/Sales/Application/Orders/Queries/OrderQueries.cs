using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Sales.Application.Orders.Dtos;

namespace Sales.Application.Orders.Queries;

[AuthorizeRequest(AuthorizationPolicies.Cashier, Permissions.SalesRead)]
public sealed class GetOrderByIdQuery : IQuery<OrderDetailDto>
{
    public Guid OrderId { get; init; }
}
