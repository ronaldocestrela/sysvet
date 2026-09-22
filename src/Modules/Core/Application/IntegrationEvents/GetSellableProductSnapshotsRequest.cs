using Core.Domain;
using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>
/// Loads catalog snapshots and available quantity for commerce storefront (Inventory module).
/// </summary>
public sealed class GetSellableProductSnapshotsRequest : IRequest<Result<IReadOnlyList<SellableProductSnapshot>>>
{
    public IReadOnlyList<Guid> ProductIds { get; }

    public GetSellableProductSnapshotsRequest(IReadOnlyList<Guid> productIds) => ProductIds = productIds;
}

/// <summary>Read model for published store offers.</summary>
public sealed class SellableProductSnapshot
{
    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public decimal AvailableQuantity { get; init; }
}
