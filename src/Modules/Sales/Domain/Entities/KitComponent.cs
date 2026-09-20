using Core.Domain;

namespace Sales.Domain.Entities;

/// <summary>Single product line inside a sellable product kit.</summary>
public sealed class KitComponent : Entity
{
    public Guid ProductKitId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal QuantityPerKit { get; private set; }

    private KitComponent() { }

    internal KitComponent(Guid id, Guid productKitId, Guid productId, decimal quantityPerKit)
        : base(id)
    {
        ProductKitId = productKitId;
        ProductId = productId;
        QuantityPerKit = quantityPerKit;
    }

    /// <summary>Creates a component row for a kit definition.</summary>
    public static Result<KitComponent> Create(Guid productKitId, Guid productId, decimal quantityPerKit)
        => Create(Guid.NewGuid(), productKitId, productId, quantityPerKit);

    /// <summary>Creates a component with a known id (sync).</summary>
    public static Result<KitComponent> Create(Guid id, Guid productKitId, Guid productId, decimal quantityPerKit)
    {
        if (productKitId == Guid.Empty || productId == Guid.Empty)
        {
            return Result.Failure<KitComponent>(ErrorCodes.Kit.InvalidComponent);
        }

        if (quantityPerKit <= 0)
        {
            return Result.Failure<KitComponent>(ErrorCodes.Kit.InvalidComponentQuantity);
        }

        return Result.Success(new KitComponent(id, productKitId, productId, quantityPerKit));
    }
}
