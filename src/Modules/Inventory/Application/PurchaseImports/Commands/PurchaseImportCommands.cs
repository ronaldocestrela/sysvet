using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Inventory.Application.PurchaseImports.Dtos;
using Inventory.Domain.Enums;

namespace Inventory.Application.PurchaseImports.Commands;

/// <summary>Uploads and parses a purchase NF-e XML into a draft import.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.PurchaseImportsWrite)]
public sealed record ParsePurchaseNfeXmlCommand(
    Stream Content,
    string FileName,
    string ContentType,
    long ContentLength,
    Guid IdempotencyKey = default) : IIdempotentCommand<PurchaseImportPreviewDto>;

/// <summary>Confirms mappings and posts purchase stock movements.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.PurchaseImportsWrite)]
public sealed record ConfirmPurchaseNfeImportCommand(
    Guid ImportId,
    ConfirmSupplierAction Supplier,
    IReadOnlyList<ConfirmLineAction> Lines,
    Guid IdempotencyKey = default) : IIdempotentCommand<Guid>;

/// <summary>Supplier resolution on confirm.</summary>
public sealed record ConfirmSupplierAction(SupplierConfirmMode Mode, Guid? SupplierId);

/// <summary>How to resolve the NF-e emitter.</summary>
public enum SupplierConfirmMode
{
    LinkExisting = 0,
    CreateFromEmitter = 1
}

/// <summary>Per-line mapping decision.</summary>
public sealed record ConfirmLineAction(
    Guid LineId,
    LineConfirmMode Mode,
    Guid? ProductId,
    CreateProductPayload? CreateProduct);

/// <summary>Line mapping mode.</summary>
public enum LineConfirmMode
{
    LinkExisting = 0,
    CreateNew = 1
}

/// <summary>Assisted product creation fields from preview.</summary>
public sealed record CreateProductPayload(
    string Sku,
    string Barcode,
    ProductCategory Category,
    bool RequiresLot,
    decimal ReorderLevel,
    string? Description);
