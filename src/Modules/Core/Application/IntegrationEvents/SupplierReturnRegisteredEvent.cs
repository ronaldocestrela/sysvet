using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>
/// Published when stock is returned to a supplier; Finance 7.x may create AP credit.
/// </summary>
public sealed class SupplierReturnRegisteredEvent : INotification
{
    public Guid MovementId { get; }
    public Guid ProductId { get; }
    public Guid SupplierId { get; }
    public decimal Quantity { get; }
    public decimal EstimatedAmount { get; }

    public SupplierReturnRegisteredEvent(
        Guid movementId,
        Guid productId,
        Guid supplierId,
        decimal quantity,
        decimal estimatedAmount)
    {
        MovementId = movementId;
        ProductId = productId;
        SupplierId = supplierId;
        Quantity = quantity;
        EstimatedAmount = estimatedAmount;
    }
}
