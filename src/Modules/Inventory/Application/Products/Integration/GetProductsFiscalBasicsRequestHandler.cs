using Core.Application.IntegrationEvents;
using Core.Domain;
using Inventory.Domain.Repositories;
using MediatR;

namespace Inventory.Application.Products.Integration;

/// <summary>Resolves NCM and origin for NF-e product lines.</summary>
public sealed class GetProductsFiscalBasicsRequestHandler
    : IRequestHandler<GetProductsFiscalBasicsRequest, Result<IReadOnlyList<ProductFiscalBasic>>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsFiscalBasicsRequestHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<IReadOnlyList<ProductFiscalBasic>>> Handle(
        GetProductsFiscalBasicsRequest request,
        CancellationToken cancellationToken)
    {
        var ids = request.ProductIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return Result.Success<IReadOnlyList<ProductFiscalBasic>>(Array.Empty<ProductFiscalBasic>());
        }

        var basics = new List<ProductFiscalBasic>();
        foreach (var id in ids)
        {
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product is null)
            {
                continue;
            }

            basics.Add(new ProductFiscalBasic
            {
                ProductId = product.Id,
                Ncm = product.Ncm,
                Cest = product.Cest,
                MerchandiseOrigin = product.MerchandiseOrigin
            });
        }

        return Result.Success<IReadOnlyList<ProductFiscalBasic>>(basics);
    }
}
