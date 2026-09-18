using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Inventory.Application.Products.Dtos;

namespace Inventory.Application.Products.Queries;

/// <summary>Lists catalog products.</summary>
[AuthorizeRequest(AuthorizationPolicies.Authenticated, Permissions.ProductsRead)]
public sealed record ListProductsQuery(bool ActiveOnly = true) : IQuery<IReadOnlyList<ProductListItemDto>>;

/// <summary>Gets product detail with lots.</summary>
[AuthorizeRequest(AuthorizationPolicies.Authenticated, Permissions.ProductsRead)]
public sealed record GetProductByIdQuery(Guid ProductId) : IQuery<ProductDetailDto>;

/// <summary>Lookup by SKU.</summary>
[AuthorizeRequest(AuthorizationPolicies.Authenticated, Permissions.ProductsRead)]
public sealed record GetProductBySkuQuery(string Sku) : IQuery<ProductDetailDto>;

/// <summary>Lookup by barcode.</summary>
[AuthorizeRequest(AuthorizationPolicies.Authenticated, Permissions.ProductsRead)]
public sealed record GetProductByBarcodeQuery(string Barcode) : IQuery<ProductDetailDto>;
