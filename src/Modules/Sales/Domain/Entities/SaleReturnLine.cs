using Core.Domain;

namespace Sales.Domain.Entities;

/// <summary>
/// Quantity returned for a specific order line.
/// </summary>
public sealed class SaleReturnLine : Entity
{
    public Guid SaleReturnId { get; private set; }
    public Guid OrderItemId { get; private set; }
    public decimal Quantity { get; private set; }

    private SaleReturnLine() { }

    internal SaleReturnLine(Guid id, Guid saleReturnId, Guid orderItemId, decimal quantity)
        : base(id)
    {
        SaleReturnId = saleReturnId;
        OrderItemId = orderItemId;
        Quantity = quantity;
    }

    /// <summary>Rehydrates from persistence or sync.</summary>
    public static SaleReturnLine Restore(Guid id, Guid saleReturnId, Guid orderItemId, decimal quantity)
        => new(id, saleReturnId, orderItemId, quantity);
}
