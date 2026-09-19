using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Inventory.Domain.Enums;

namespace Inventory.Application.Products.Commands;

/// <summary>Registers a new catalog product.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.ProductsWrite)]
public sealed record RegisterProductCommand(
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
    bool? RequiresLot,
    decimal? UnitsPerPackage = null,
    decimal TargetStock = 0m,
    Guid ProductId = default,
    Guid IdempotencyKey = default) : IIdempotentCommand<Guid>;
