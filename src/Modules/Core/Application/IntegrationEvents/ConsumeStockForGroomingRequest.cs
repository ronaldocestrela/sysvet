using Core.Domain;
using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>
/// Synchronous stock debit when a grooming appointment is completed (handled by Inventory).
/// </summary>
public sealed class ConsumeStockForGroomingRequest : IRequest<Result>
{
    public Guid AttendanceId { get; }
    public IReadOnlyList<ConsumeStockForGroomingLine> Lines { get; }

    public ConsumeStockForGroomingRequest(Guid attendanceId, IReadOnlyList<ConsumeStockForGroomingLine> lines)
    {
        AttendanceId = attendanceId;
        Lines = lines;
    }
}

/// <summary>Product line consumed from inventory on grooming completion.</summary>
public sealed class ConsumeStockForGroomingLine
{
    public Guid ProductId { get; }
    public decimal Quantity { get; }

    public ConsumeStockForGroomingLine(Guid productId, decimal quantity)
    {
        ProductId = productId;
        Quantity = quantity;
    }
}
