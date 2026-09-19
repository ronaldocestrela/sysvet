using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Sales.Domain.Enums;

namespace Sales.Application.Orders.Commands;

[AuthorizeRequest(AuthorizationPolicies.Cashier, Permissions.SalesWrite)]
public class CreateOrderCommand : ICommand<Guid>, IIdempotentCommand<Guid>
{
    /// <summary>Client-assigned order id for offline sync; optional on REST create.</summary>
    public Guid? OrderId { get; set; }

    public Guid CashRegisterId { get; set; }
    public Guid? TutorId { get; set; }
    public Guid? PetId { get; set; }
    public Guid? SourceQuoteId { get; set; }
    public Guid? SellerUserId { get; set; }
    public decimal DiscountPercent { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
    public Guid IdempotencyKey { get; set; }
}

public class CreateOrderItemDto
{
    public OrderItemKind Kind { get; set; } = OrderItemKind.Product;
    public Guid? ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public Guid? PerformerUserId { get; set; }
    public CommissionRole? PerformerRole { get; set; }
}
