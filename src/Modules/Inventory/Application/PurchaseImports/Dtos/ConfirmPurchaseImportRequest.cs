using Inventory.Application.PurchaseImports.Commands;

namespace Inventory.Application.PurchaseImports.Dtos;

/// <summary>HTTP body for confirming a purchase NF-e import.</summary>
public sealed class ConfirmPurchaseImportRequest
{
    public ConfirmSupplierAction Supplier { get; init; } = new(SupplierConfirmMode.CreateFromEmitter, null);
    public IReadOnlyList<ConfirmLineAction> Lines { get; init; } = Array.Empty<ConfirmLineAction>();
}
