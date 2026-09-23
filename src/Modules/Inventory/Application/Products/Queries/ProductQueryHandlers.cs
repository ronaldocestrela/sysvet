using Core.Application.Common;
using Core.Domain;
using Inventory.Application.Products.Dtos;
using Inventory.Domain.Repositories;
using MediatR;

namespace Inventory.Application.Products.Queries;

/// <summary>Lists products with total quantity.</summary>
public sealed class ListProductsQueryHandler : IRequestHandler<ListProductsQuery, Result<PagedResult<ProductListItemDto>>>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductLotRepository _lotRepository;

    public ListProductsQueryHandler(IProductRepository productRepository, IProductLotRepository lotRepository)
    {
        _productRepository = productRepository;
        _lotRepository = lotRepository;
    }

    public async Task<Result<PagedResult<ProductListItemDto>>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
    {
        var pageRequest = PageRequest.TryCreate(request.Page, request.PageSize);
        if (pageRequest.IsFailure)
        {
            return Result.Failure<PagedResult<ProductListItemDto>>(pageRequest.Error);
        }

        var (page, pageSize) = (pageRequest.Value.Page, pageRequest.Value.PageSize);
        var (products, totalCount) = await _productRepository.ListPagedAsync(
            request.ActiveOnly,
            request.Search,
            page,
            pageSize,
            cancellationToken);

        var items = new List<ProductListItemDto>();
        foreach (var product in products)
        {
            var lots = await _lotRepository.ListByProductIdAsync(product.Id, cancellationToken);
            var totalFromLots = lots.Where(l => l.IsActive).Sum(l => l.Quantity);
            var balance = await _productRepository.GetBalanceAsync(product.Id, cancellationToken);
            var total = lots.Count > 0 ? totalFromLots : balance?.TotalQuantity ?? 0m;
            items.Add(new ProductListItemDto(
                product.Id,
                product.Name,
                product.Sku,
                product.Barcode,
                product.Category,
                total,
                product.AverageCost,
                product.ReorderLevel,
                product.TargetStock,
                product.IsActive));
        }

        return Result.Success(new PagedResult<ProductListItemDto>(items, page, pageSize, totalCount));
    }
}

/// <summary>Gets product by id.</summary>
public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDetailDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductLotRepository _lotRepository;

    public GetProductByIdQueryHandler(IProductRepository productRepository, IProductLotRepository lotRepository)
    {
        _productRepository = productRepository;
        _lotRepository = lotRepository;
    }

    public async Task<Result<ProductDetailDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure<ProductDetailDto>(Inventory.Domain.ErrorCodes.Product.NotFound);
        }

        return Result.Success(await MapDetailAsync(_productRepository, _lotRepository, product, cancellationToken));
    }

    internal static async Task<ProductDetailDto> MapDetailAsync(
        IProductRepository productRepository,
        IProductLotRepository lotRepository,
        Domain.Entities.Product product,
        CancellationToken cancellationToken)
    {
        var balance = await productRepository.GetBalanceAsync(product.Id, cancellationToken);
        var lots = await lotRepository.ListByProductIdAsync(product.Id, cancellationToken);
        var activeLots = lots.Where(l => l.IsActive).ToList();
        var lotDtos = lots.Select(l => new ProductLotDto(l.Id, l.LotNumber, l.ExpirationDate, l.UnitCost, l.Quantity, l.IsActive, l.IsFractional)).ToList();
        var totalFromLots = activeLots.Sum(l => l.Quantity);
        var sealedQty = activeLots.Where(l => !l.IsFractional).Sum(l => l.Quantity);
        var fractionalQty = activeLots.Where(l => l.IsFractional).Sum(l => l.Quantity);
        var total = lots.Count > 0 ? totalFromLots : balance?.TotalQuantity ?? 0m;

        return new ProductDetailDto(
            product.Id,
            product.Name,
            product.Description,
            product.Sku,
            product.Barcode,
            product.UnitOfMeasure,
            product.ReorderLevel,
            product.TargetStock,
            product.Category,
            product.SupplierId,
            product.Ncm,
            product.Cest,
            product.MerchandiseOrigin,
            product.AverageCost,
            total,
            product.RequiresLot,
            product.UnitsPerPackage,
            sealedQty,
            fractionalQty,
            product.IsActive,
            lotDtos);
    }

}

/// <summary>Gets product by SKU.</summary>
public sealed class GetProductBySkuQueryHandler : IRequestHandler<GetProductBySkuQuery, Result<ProductDetailDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMediator _mediator;

    public GetProductBySkuQueryHandler(IProductRepository productRepository, IMediator mediator)
    {
        _productRepository = productRepository;
        _mediator = mediator;
    }

    public async Task<Result<ProductDetailDto>> Handle(GetProductBySkuQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetBySkuAsync(request.Sku, cancellationToken);
        if (product is null)
        {
            return Result.Failure<ProductDetailDto>(Inventory.Domain.ErrorCodes.Product.NotFound);
        }

        return await _mediator.Send(new GetProductByIdQuery(product.Id), cancellationToken);
    }
}

/// <summary>Gets product by barcode.</summary>
public sealed class GetProductByBarcodeQueryHandler : IRequestHandler<GetProductByBarcodeQuery, Result<ProductDetailDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMediator _mediator;

    public GetProductByBarcodeQueryHandler(IProductRepository productRepository, IMediator mediator)
    {
        _productRepository = productRepository;
        _mediator = mediator;
    }

    public async Task<Result<ProductDetailDto>> Handle(GetProductByBarcodeQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByBarcodeAsync(request.Barcode, cancellationToken);
        if (product is null)
        {
            return Result.Failure<ProductDetailDto>(Inventory.Domain.ErrorCodes.Product.NotFound);
        }

        return await _mediator.Send(new GetProductByIdQuery(product.Id), cancellationToken);
    }
}
