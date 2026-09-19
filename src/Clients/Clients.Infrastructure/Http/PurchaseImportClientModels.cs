using Inventory.Domain.Enums;

namespace Clients.Infrastructure.Http;

/// <summary>Client mirror of purchase import preview from API.</summary>
public sealed class PurchaseImportPreviewClientDto
{
    public Guid ImportId { get; init; }
    public string AccessKey { get; init; } = string.Empty;
    public string InvoiceNumber { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public Guid? MatchedSupplierId { get; init; }
    public PurchaseImportEmitterClientDto Emitter { get; init; } = new();
    public IReadOnlyList<PurchaseImportLinePreviewClientDto> Lines { get; init; } = Array.Empty<PurchaseImportLinePreviewClientDto>();
}

public sealed class PurchaseImportEmitterClientDto
{
    public string LegalName { get; init; } = string.Empty;
    public string TradeName { get; init; } = string.Empty;
    public string Document { get; init; } = string.Empty;
}

public sealed class PurchaseImportLinePreviewClientDto
{
    public Guid LineId { get; init; }
    public string Description { get; init; } = string.Empty;
    public string SupplierProductCode { get; init; } = string.Empty;
    public string? Barcode { get; init; }
    public string Ncm { get; init; } = string.Empty;
    public string UnitOfMeasure { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal UnitCost { get; init; }
    public string? LotNumber { get; init; }
    public Guid? SuggestedProductId { get; init; }
    public string? SuggestedSku { get; init; }
    public bool RequiresMapping { get; init; }
}

public sealed class PurchaseImportDetailClientDto
{
    public Guid Id { get; init; }
    public string AccessKey { get; init; } = string.Empty;
    public PurchaseImportStatus Status { get; init; }
    public decimal TotalAmount { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public IReadOnlyList<PurchaseImportLineDetailClientDto> Lines { get; init; } = Array.Empty<PurchaseImportLineDetailClientDto>();
}

public sealed class PurchaseImportLineDetailClientDto
{
    public Guid LineId { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public Guid? ProductId { get; init; }
    public Guid? StockMovementId { get; init; }
    public string? LotNumber { get; init; }
}

public sealed class ConfirmPurchaseImportClientRequest
{
    public ConfirmSupplierClientAction Supplier { get; init; } = new();
    public IReadOnlyList<ConfirmLineClientAction> Lines { get; init; } = Array.Empty<ConfirmLineClientAction>();
}

public sealed class ConfirmSupplierClientAction
{
    public int Mode { get; init; }
    public Guid? SupplierId { get; init; }
}

public sealed class ConfirmLineClientAction
{
    public Guid LineId { get; init; }
    public int Mode { get; init; }
    public Guid? ProductId { get; init; }
    public CreateProductClientPayload? CreateProduct { get; init; }
}

public sealed class CreateProductClientPayload
{
    public string Sku { get; init; } = string.Empty;
    public string Barcode { get; init; } = string.Empty;
    public int Category { get; init; }
    public bool RequiresLot { get; init; }
    public decimal ReorderLevel { get; init; }
    public string? Description { get; init; }
}
