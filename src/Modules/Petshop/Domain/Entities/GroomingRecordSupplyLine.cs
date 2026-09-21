using Core.Domain;

namespace Petshop.Domain.Entities;

/// <summary>
/// Inventory product line on a grooming digital record (editable per visit).
/// </summary>
public sealed class GroomingRecordSupplyLine : Entity
{
    public Guid GroomingRecordId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }

    private GroomingRecordSupplyLine() { }

    internal GroomingRecordSupplyLine(Guid id, Guid groomingRecordId, Guid productId, decimal quantity)
        : base(id)
    {
        GroomingRecordId = groomingRecordId;
        ProductId = productId;
        Quantity = quantity;
    }

    /// <summary>Creates a supply line when quantity is positive.</summary>
    public static Result<GroomingRecordSupplyLine> Create(Guid groomingRecordId, Guid productId, decimal quantity)
    {
        if (groomingRecordId == Guid.Empty || productId == Guid.Empty || quantity <= 0)
        {
            return Result.Failure<GroomingRecordSupplyLine>(ErrorCodes.GroomingRecord.InvalidSupply);
        }

        return Result.Success(new GroomingRecordSupplyLine(Guid.NewGuid(), groomingRecordId, productId, quantity));
    }

    /// <summary>Updates quantity on a draft record line.</summary>
    internal Result UpdateQuantity(decimal quantity)
    {
        if (quantity <= 0)
        {
            return Result.Failure(ErrorCodes.GroomingRecord.InvalidSupply);
        }

        Quantity = quantity;
        Touch();
        return Result.Success();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
