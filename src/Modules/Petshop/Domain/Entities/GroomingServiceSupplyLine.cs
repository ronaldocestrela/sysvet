using Core.Domain;

namespace Petshop.Domain.Entities;

/// <summary>
/// Default inventory product usage for a catalog grooming service.
/// </summary>
public sealed class GroomingServiceSupplyLine : Entity
{
    public Guid GroomingServiceId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }

    private GroomingServiceSupplyLine() { }

    internal GroomingServiceSupplyLine(Guid id, Guid groomingServiceId, Guid productId, decimal quantity)
        : base(id)
    {
        GroomingServiceId = groomingServiceId;
        ProductId = productId;
        Quantity = quantity;
    }

    /// <summary>Creates a supply line when quantity is positive.</summary>
    public static Result<GroomingServiceSupplyLine> Create(Guid groomingServiceId, Guid productId, decimal quantity)
    {
        if (groomingServiceId == Guid.Empty || productId == Guid.Empty || quantity <= 0)
        {
            return Result.Failure<GroomingServiceSupplyLine>(ErrorCodes.GroomingService.InvalidSupply);
        }

        return Result.Success(new GroomingServiceSupplyLine(Guid.NewGuid(), groomingServiceId, productId, quantity));
    }
}
