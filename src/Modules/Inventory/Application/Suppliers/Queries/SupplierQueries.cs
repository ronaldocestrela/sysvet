using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Inventory.Application.Suppliers.Dtos;

namespace Inventory.Application.Suppliers.Queries;

[AuthorizeRequest(AuthorizationPolicies.Authenticated, Permissions.ProductsRead)]
public sealed record ListSuppliersQuery(bool ActiveOnly = true) : IQuery<IReadOnlyList<SupplierDto>>;

[AuthorizeRequest(AuthorizationPolicies.Authenticated, Permissions.ProductsRead)]
public sealed record GetSupplierByIdQuery(Guid SupplierId) : IQuery<SupplierDto>;
