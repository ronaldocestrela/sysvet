using Core.Domain;
using Sales.Domain.ValueObjects;

namespace Sales.Domain.Entities;

/// <summary>
/// Customer return of sold items against a paid order.
/// </summary>
public sealed class SaleReturn : Entity
{
    public Guid OrderId { get; internal set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Money RefundAmount { get; private set; } = Money.Zero;

    private readonly List<SaleReturnLine> _lines = new();
    public IReadOnlyCollection<SaleReturnLine> Lines => _lines.AsReadOnly();

    private SaleReturn() { }

    private SaleReturn(Guid id, Guid orderId, decimal refundAmount, IEnumerable<SaleReturnLine> lines)
        : base(id)
    {
        OrderId = orderId;
        RefundAmount = Money.CreateUnsafe(refundAmount);
        CreatedAt = DateTimeOffset.UtcNow;
        _lines.AddRange(lines);
    }

    /// <summary>Creates a return header with line snapshots.</summary>
    public static SaleReturn Create(
        Guid id,
        Guid orderId,
        decimal refundAmount,
        IEnumerable<(Guid LineId, Guid OrderItemId, decimal Quantity)> lines)
    {
        var lineEntities = lines
            .Select(l => SaleReturnLine.Restore(l.LineId, id, l.OrderItemId, l.Quantity))
            .ToList();

        return new SaleReturn(id, orderId, refundAmount, lineEntities);
    }

    /// <summary>Rehydrates from persistence or sync.</summary>
    public static SaleReturn Restore(
        Guid id,
        Guid orderId,
        decimal refundAmount,
        DateTimeOffset createdAt,
        IEnumerable<(Guid LineId, Guid OrderItemId, decimal Quantity)> lines)
    {
        var entity = new SaleReturn(id, orderId, refundAmount, lines.Select(l => SaleReturnLine.Restore(l.LineId, id, l.OrderItemId, l.Quantity)))
        {
            CreatedAt = createdAt
        };
        return entity;
    }
}
