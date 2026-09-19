using Inventory.Domain.Enums;

namespace Inventory.Application.PurchaseImports.Dtos;

/// <summary>
/// Preview returned after XML parse with suggested matches.
/// </summary>
public sealed class PurchaseImportPreviewDto
{
    public Guid ImportId { get; init; }
    public string AccessKey { get; init; } = string.Empty;
    public string InvoiceNumber { get; init; } = string.Empty;
    public string InvoiceSeries { get; init; } = string.Empty;
    public DateTimeOffset IssuedAt { get; init; }
    public decimal TotalAmount { get; init; }
    public PurchaseImportSupplierPreviewDto Emitter { get; init; } = new();
    public Guid? MatchedSupplierId { get; init; }
    public IReadOnlyList<PurchaseImportLinePreviewDto> Lines { get; init; } = Array.Empty<PurchaseImportLinePreviewDto>();
}

/// <summary>
/// Emitter snapshot and optional existing supplier link.
/// </summary>
public sealed class PurchaseImportSupplierPreviewDto
{
    public string LegalName { get; init; } = string.Empty;
    public string TradeName { get; init; } = string.Empty;
    public string Document { get; init; } = string.Empty;
}

/// <summary>
/// One import line with match hints for assisted mapping.
/// </summary>
public sealed class PurchaseImportLinePreviewDto
{
    public Guid LineId { get; init; }
    public int ItemNumber { get; init; }
    public string SupplierProductCode { get; init; } = string.Empty;
    public string? Barcode { get; init; }
    public string Description { get; init; } = string.Empty;
    public string Ncm { get; init; } = string.Empty;
    public string UnitOfMeasure { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal UnitCost { get; init; }
    public string? LotNumber { get; init; }
    public DateTimeOffset? ExpirationDate { get; init; }
    public Guid? SuggestedProductId { get; init; }
    public string? SuggestedSku { get; init; }
    public bool RequiresMapping { get; init; }
}

/// <summary>
/// Import header for list and detail conference views.
/// </summary>
public sealed class PurchaseImportDetailDto
{
    public Guid Id { get; init; }
    public string AccessKey { get; init; } = string.Empty;
    public PurchaseImportStatus Status { get; init; }
    public ApIntegrationStatus ApIntegrationStatus { get; init; }
    public Guid? SupplierId { get; init; }
    public string EmitterLegalName { get; init; } = string.Empty;
    public string InvoiceNumber { get; init; } = string.Empty;
    public string InvoiceSeries { get; init; } = string.Empty;
    public DateTimeOffset IssuedAt { get; init; }
    public decimal TotalAmount { get; init; }
    public IReadOnlyList<PurchaseImportLineDetailDto> Lines { get; init; } = Array.Empty<PurchaseImportLineDetailDto>();
}

/// <summary>
/// Line with post-confirm stock linkage for conference.
/// </summary>
public sealed class PurchaseImportLineDetailDto
{
    public Guid LineId { get; init; }
    public int ItemNumber { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal UnitCost { get; init; }
    public Guid? ProductId { get; init; }
    public Guid? ProductLotId { get; init; }
    public Guid? StockMovementId { get; init; }
    public string? LotNumber { get; init; }
}

/// <summary>
/// Summary row for import list.
/// </summary>
public sealed class PurchaseImportListItemDto
{
    public Guid Id { get; init; }
    public string AccessKey { get; init; } = string.Empty;
    public PurchaseImportStatus Status { get; init; }
    public string EmitterLegalName { get; init; } = string.Empty;
    public DateTimeOffset IssuedAt { get; init; }
    public decimal TotalAmount { get; init; }
}
