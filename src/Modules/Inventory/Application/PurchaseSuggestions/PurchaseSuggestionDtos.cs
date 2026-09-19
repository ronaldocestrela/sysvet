namespace Inventory.Application.PurchaseSuggestions;

/// <summary>One product line in a purchase suggestion report.</summary>
public sealed record PurchaseSuggestionLineDto(
    Guid ProductId,
    string ProductName,
    string Sku,
    string Barcode,
    decimal OnHand,
    decimal ReorderLevel,
    decimal TargetStock,
    decimal SuggestedQuantity,
    decimal AverageCost,
    decimal EstimatedTotal);

/// <summary>Suggested lines grouped by supplier.</summary>
public sealed record PurchaseSuggestionGroupDto(
    Guid? SupplierId,
    string SupplierName,
    string? SupplierDocument,
    IReadOnlyList<PurchaseSuggestionLineDto> Lines,
    decimal GroupEstimatedTotal);
