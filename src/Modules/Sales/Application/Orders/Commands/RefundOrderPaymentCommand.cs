using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Sales.Application.Orders.Commands;

/// <summary>Records a partial or full estorno against a payment line on a paid order.</summary>
[AuthorizeRequest(AuthorizationPolicies.Cashier, Permissions.SalesWrite)]
public class RefundOrderPaymentCommand : ICommand<Guid>, IIdempotentCommand<Guid>
{
    public Guid OrderId { get; set; }
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string? RefundNsu { get; set; }
    public Guid IdempotencyKey { get; set; }
}
