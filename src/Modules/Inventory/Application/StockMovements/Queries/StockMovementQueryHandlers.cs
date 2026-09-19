using Core.Domain;
using Inventory.Application.StockMovements.Dtos;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using Inventory.Domain.Services;
using MediatR;

namespace Inventory.Application.StockMovements.Queries;

/// <summary>Lists recent stock movements.</summary>
public sealed class ListStockMovementsQueryHandler : IRequestHandler<ListStockMovementsQuery, Result<IReadOnlyList<StockMovementListItemDto>>>
{
    private readonly IStockMovementRepository _movementRepository;
    private readonly IProductRepository _productRepository;
    private readonly IProductLotRepository _lotRepository;

    public ListStockMovementsQueryHandler(
        IStockMovementRepository movementRepository,
        IProductRepository productRepository,
        IProductLotRepository lotRepository)
    {
        _movementRepository = movementRepository;
        _productRepository = productRepository;
        _lotRepository = lotRepository;
    }

    public async Task<Result<IReadOnlyList<StockMovementListItemDto>>> Handle(ListStockMovementsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var take = Math.Clamp(request.PageSize, 1, 200);
        var skip = (page - 1) * take;

        var movements = await _movementRepository.ListRecentAsync(request.ProductId, request.Reason, skip, take, cancellationToken);
        var products = (await _productRepository.GetAllAsync(cancellationToken)).ToDictionary(p => p.Id);
        var lotCache = new Dictionary<Guid, ProductLot>();

        var items = new List<StockMovementListItemDto>();
        foreach (var movement in movements)
        {
            products.TryGetValue(movement.ProductId, out var product);
            string? lotNumber = null;
            if (movement.ProductLotId is Guid lotId)
            {
                if (!lotCache.TryGetValue(lotId, out var lot))
                {
                    lot = await _lotRepository.GetByIdAsync(lotId, cancellationToken);
                    if (lot is not null)
                    {
                        lotCache[lotId] = lot;
                    }
                }

                lotNumber = lot?.LotNumber;
            }

            items.Add(new StockMovementListItemDto(
                movement.Id,
                movement.ProductId,
                product?.Name ?? string.Empty,
                movement.ProductLotId,
                lotNumber,
                movement.Type.ToString(),
                movement.AdjustmentDirection?.ToString(),
                movement.Quantity,
                movement.Reason,
                movement.Date,
                movement.CorrelationId));
        }

        return Result.Success<IReadOnlyList<StockMovementListItemDto>>(items);
    }
}

/// <summary>Builds kardex with running balance for a product.</summary>
public sealed class GetProductKardexQueryHandler : IRequestHandler<GetProductKardexQuery, Result<IReadOnlyList<ProductKardexLineDto>>>
{
    private readonly IProductRepository _productRepository;
    private readonly IStockMovementRepository _movementRepository;
    private readonly IProductLotRepository _lotRepository;

    public GetProductKardexQueryHandler(
        IProductRepository productRepository,
        IStockMovementRepository movementRepository,
        IProductLotRepository lotRepository)
    {
        _productRepository = productRepository;
        _movementRepository = movementRepository;
        _lotRepository = lotRepository;
    }

    public async Task<Result<IReadOnlyList<ProductKardexLineDto>>> Handle(GetProductKardexQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure<IReadOnlyList<ProductKardexLineDto>>(Inventory.Domain.ErrorCodes.Product.NotFound);
        }

        var all = await _movementRepository.ListByProductAsync(request.ProductId, cancellationToken);
        if (request.ProductLotId is Guid lotFilter)
        {
            all = all.Where(m => m.ProductLotId == lotFilter).ToList();
        }

        if (request.From is not null)
        {
            all = all.Where(m => m.Date >= request.From.Value).ToList();
        }

        if (request.To is not null)
        {
            all = all.Where(m => m.Date <= request.To.Value).ToList();
        }

        var ordered = all.OrderBy(m => m.Date).ThenBy(m => m.Id).ToList();
        var lotCache = new Dictionary<Guid, ProductLot>();
        decimal running = 0m;
        var lines = new List<ProductKardexLineDto>();

        foreach (var movement in ordered)
        {
            var deltaResult = StockQuantityApplier.ResolveDelta(movement.Type, movement.Quantity, movement.AdjustmentDirection);
            if (deltaResult.IsSuccess)
            {
                running += deltaResult.Value;
            }

            string? lotNumber = null;
            if (movement.ProductLotId is Guid lotId)
            {
                if (!lotCache.TryGetValue(lotId, out var lot))
                {
                    lot = await _lotRepository.GetByIdAsync(lotId, cancellationToken);
                    if (lot is not null)
                    {
                        lotCache[lotId] = lot;
                    }
                }

                lotNumber = lot?.LotNumber;
            }

            lines.Add(new ProductKardexLineDto(
                movement.Id,
                movement.Date,
                movement.Type.ToString(),
                movement.AdjustmentDirection?.ToString(),
                movement.Quantity,
                running,
                movement.Reason,
                movement.ProductLotId,
                lotNumber,
                movement.CorrelationId));
        }

        return Result.Success<IReadOnlyList<ProductKardexLineDto>>(lines);
    }
}

/// <summary>Projects low-stock and expiry alerts.</summary>
public sealed class ListStockAlertsQueryHandler : IRequestHandler<ListStockAlertsQuery, Result<IReadOnlyList<StockAlertDto>>>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductLotRepository _lotRepository;

    public ListStockAlertsQueryHandler(IProductRepository productRepository, IProductLotRepository lotRepository)
    {
        _productRepository = productRepository;
        _lotRepository = lotRepository;
    }

    public async Task<Result<IReadOnlyList<StockAlertDto>>> Handle(ListStockAlertsQuery request, CancellationToken cancellationToken)
    {
        var horizon = Math.Clamp(request.HorizonDays, 1, 365);
        var utcNow = DateTimeOffset.UtcNow;
        var products = (await _productRepository.GetAllAsync(cancellationToken)).Where(p => p.IsActive).ToList();
        var alerts = new List<StockAlertDto>();

        foreach (var product in products)
        {
            var lots = await _lotRepository.ListByProductIdAsync(product.Id, cancellationToken);
            var totalFromLots = lots.Where(l => l.IsActive).Sum(l => l.Quantity);
            var balance = await _productRepository.GetBalanceAsync(product.Id, cancellationToken);
            var total = lots.Count > 0 ? totalFromLots : balance?.TotalQuantity ?? 0m;

            if (request.Kind is null or StockAlertKind.LowStock &&
                StockAlertClassifier.IsLowStock(total, product.ReorderLevel))
            {
                alerts.Add(new StockAlertDto(
                    StockAlertKind.LowStock.ToString(),
                    product.Id,
                    product.Name,
                    product.Sku,
                    null,
                    null,
                    total,
                    product.ReorderLevel,
                    null));
            }

            foreach (var lot in lots.Where(l => l.IsActive && l.Quantity > 0))
            {
                var expiryKind = StockAlertClassifier.ClassifyLotExpiry(lot.ExpirationDate, utcNow, horizon);
                if (expiryKind == StockAlertKind.None)
                {
                    continue;
                }

                if (request.Kind is not null && request.Kind != expiryKind)
                {
                    continue;
                }

                alerts.Add(new StockAlertDto(
                    expiryKind.ToString(),
                    product.Id,
                    product.Name,
                    product.Sku,
                    lot.Id,
                    lot.LotNumber,
                    lot.Quantity,
                    product.ReorderLevel,
                    lot.ExpirationDate));
            }
        }

        var page = Math.Max(1, request.Page);
        var take = Math.Clamp(request.PageSize, 1, 200);
        var skip = (page - 1) * take;
        var pageItems = alerts
            .OrderBy(a => a.Kind)
            .ThenBy(a => a.ProductName)
            .Skip(skip)
            .Take(take)
            .ToList();

        return Result.Success<IReadOnlyList<StockAlertDto>>(pageItems);
    }
}
