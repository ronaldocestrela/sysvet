using Core.Application.IntegrationEvents;
using Core.Domain;
using Inventory.Domain.Repositories;
using MediatR;

namespace Inventory.Application.Products.Integration;

/// <summary>Resolves product name, SKU, active flag and on-hand quantity for commerce.</summary>
public sealed class GetSellableProductSnapshotsRequestHandler
    : IRequestHandler<GetSellableProductSnapshotsRequest, Result<IReadOnlyList<SellableProductSnapshot>>>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductLotRepository _lotRepository;

    public GetSellableProductSnapshotsRequestHandler(
        IProductRepository productRepository,
        IProductLotRepository lotRepository)
    {
        _productRepository = productRepository;
        _lotRepository = lotRepository;
    }

    public async Task<Result<IReadOnlyList<SellableProductSnapshot>>> Handle(
        GetSellableProductSnapshotsRequest request,
        CancellationToken cancellationToken)
    {
        var ids = request.ProductIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return Result.Success<IReadOnlyList<SellableProductSnapshot>>(Array.Empty<SellableProductSnapshot>());
        }

        var snapshots = new List<SellableProductSnapshot>();
        foreach (var id in ids)
        {
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product is null)
            {
                continue;
            }

            var lots = await _lotRepository.ListByProductIdAsync(product.Id, cancellationToken);
            var totalFromLots = lots.Where(l => l.IsActive).Sum(l => l.Quantity);
            var balance = await _productRepository.GetBalanceAsync(product.Id, cancellationToken);
            var available = lots.Count > 0 ? totalFromLots : balance?.TotalQuantity ?? 0m;

            snapshots.Add(new SellableProductSnapshot
            {
                ProductId = product.Id,
                Name = product.Name,
                Sku = product.Sku,
                IsActive = product.IsActive,
                AvailableQuantity = available
            });
        }

        return Result.Success<IReadOnlyList<SellableProductSnapshot>>(snapshots);
    }
}
