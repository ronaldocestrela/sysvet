using Core.Domain;
using Inventory.Application.InventoryCounts;
using Inventory.Application.InventoryCounts.Dtos;
using InvErrors = Inventory.Domain.ErrorCodes;
using Inventory.Domain.Repositories;
using MediatR;

namespace Inventory.Application.InventoryCounts.Queries;

/// <summary>Lists recent inventory sessions.</summary>
public sealed class ListInventoryCountsQueryHandler : IRequestHandler<ListInventoryCountsQuery, Result<IReadOnlyList<InventoryCountListItemDto>>>
{
    private readonly IInventoryCountRepository _countRepository;

    public ListInventoryCountsQueryHandler(IInventoryCountRepository countRepository) => _countRepository = countRepository;

    public async Task<Result<IReadOnlyList<InventoryCountListItemDto>>> Handle(ListInventoryCountsQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _countRepository.ListRecentAsync(request.Take, cancellationToken);
        var items = sessions.Select(session => new InventoryCountListItemDto
        {
            Id = session.Id,
            Code = session.Code,
            Status = session.Status,
            UpdatedAt = session.UpdatedAt,
            LineCount = session.Lines.Count
        }).ToList();

        return Result.Success<IReadOnlyList<InventoryCountListItemDto>>(items);
    }
}

/// <summary>Loads session detail with product labels.</summary>
public sealed class GetInventoryCountByIdQueryHandler : IRequestHandler<GetInventoryCountByIdQuery, Result<InventoryCountDetailDto>>
{
    private readonly IInventoryCountRepository _countRepository;
    private readonly IProductRepository _productRepository;
    private readonly IProductLotRepository _lotRepository;

    public GetInventoryCountByIdQueryHandler(
        IInventoryCountRepository countRepository,
        IProductRepository productRepository,
        IProductLotRepository lotRepository)
    {
        _countRepository = countRepository;
        _productRepository = productRepository;
        _lotRepository = lotRepository;
    }

    public async Task<Result<InventoryCountDetailDto>> Handle(GetInventoryCountByIdQuery request, CancellationToken cancellationToken)
    {
        var session = await _countRepository.GetByIdWithLinesAsync(request.InventoryCountId, cancellationToken);
        if (session is null)
        {
            return Result.Failure<InventoryCountDetailDto>(InvErrors.InventoryCount.NotFound);
        }

        var productLabels = new Dictionary<Guid, (string Name, string Sku)>();
        var lotNumbers = new Dictionary<Guid, string>();
        foreach (var line in session.Lines)
        {
            if (!productLabels.ContainsKey(line.ProductId))
            {
                var product = await _productRepository.GetByIdAsync(line.ProductId, cancellationToken);
                if (product is not null)
                {
                    productLabels[line.ProductId] = (product.Name, product.Sku);
                }
            }

            if (line.ProductLotId is not null && !lotNumbers.ContainsKey(line.ProductLotId.Value))
            {
                var lot = await _lotRepository.GetByIdAsync(line.ProductLotId.Value, cancellationToken);
                if (lot is not null)
                {
                    lotNumbers[line.ProductLotId.Value] = lot.LotNumber;
                }
            }
        }

        return Result.Success(InventoryCountMapper.MapDetail(session, productLabels, lotNumbers));
    }
}
