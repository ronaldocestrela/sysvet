using Core.Domain;
using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>Loads product NCM/origin for NF-e lines (Inventory module).</summary>
public sealed class GetProductsFiscalBasicsRequest : IRequest<Result<IReadOnlyList<ProductFiscalBasic>>>
{
    public IReadOnlyList<Guid> ProductIds { get; }

    public GetProductsFiscalBasicsRequest(IReadOnlyList<Guid> productIds) => ProductIds = productIds;
}

/// <summary>Product fiscal fields from catalog.</summary>
public sealed class ProductFiscalBasic
{
    public Guid ProductId { get; init; }
    public string Ncm { get; init; } = string.Empty;
    public string? Cest { get; init; }
    public int MerchandiseOrigin { get; init; }
}
