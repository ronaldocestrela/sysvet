using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Inventory.Domain.Enums;

namespace Inventory.Application.Products.Commands;

/// <summary>Updates catalog product fields.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.ProductsWrite)]
public sealed record UpdateProductCommand(
    Guid ProductId,
    string Name,
    string Description,
    string Sku,
    string Barcode,
    string UnitOfMeasure,
    decimal ReorderLevel,
    ProductCategory Category,
    string Ncm,
    string? Cest,
    int MerchandiseOrigin,
    Guid? SupplierId,
    bool RequiresLot,
    Guid IdempotencyKey = default) : IIdempotentCommand;
