using Core.Domain;
using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>
/// Synchronous stock credit for returned product lines on a sale (handled by Inventory before commit).
/// </summary>
public sealed class RestoreStockForSaleReturnRequest : IRequest<Result>
{
    public Guid OrderId { get; }
    public Guid ReturnId { get; }
    public IReadOnlyList<RestoreStockForSaleReturnLine> Lines { get; }

    public RestoreStockForSaleReturnRequest(Guid orderId, Guid returnId, IReadOnlyList<RestoreStockForSaleReturnLine> lines)
    {
        OrderId = orderId;
        ReturnId = returnId;
        Lines = lines;
    }
}

/// <summary>Product quantity to restore on a customer return.</summary>
public sealed class RestoreStockForSaleReturnLine
{
    public Guid ProductId { get; }
    public decimal Quantity { get; }

    public RestoreStockForSaleReturnLine(Guid productId, decimal quantity)
    {
        ProductId = productId;
        Quantity = quantity;
    }
}
