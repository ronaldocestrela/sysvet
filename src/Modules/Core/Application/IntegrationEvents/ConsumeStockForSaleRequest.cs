using Core.Domain;
using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>
/// Synchronous stock debit for a paid order (handled by Inventory before commit).
/// </summary>
public sealed class ConsumeStockForSaleRequest : IRequest<Result>
{
    public Guid OrderId { get; }
    public IReadOnlyList<ConsumeStockForSaleLine> Lines { get; }

    public ConsumeStockForSaleRequest(Guid orderId, IReadOnlyList<ConsumeStockForSaleLine> lines)
    {
        OrderId = orderId;
        Lines = lines;
    }
}

/// <summary>Product line to consume from inventory on sale completion.</summary>
public sealed class ConsumeStockForSaleLine
{
    public Guid ProductId { get; }
    public decimal Quantity { get; }

    public ConsumeStockForSaleLine(Guid productId, decimal quantity)
    {
        ProductId = productId;
        Quantity = quantity;
    }
}
