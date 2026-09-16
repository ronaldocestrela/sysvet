using Core.Domain;

namespace Inventory.Domain;

/// <summary>
/// Standardized error codes for the Inventory module.
/// </summary>
public static class ErrorCodes
{
    public static class Product
    {
        public static readonly Error InvalidName = new("Product.InvalidName", "Product name cannot be empty.");
        public static readonly Error InvalidBarcode = new("Product.InvalidBarcode", "Product barcode cannot be empty.");
        public static readonly Error InvalidReorderLevel = new("Product.InvalidReorderLevel", "Reorder level cannot be negative.");
        public static readonly Error NotFound = new("Product.NotFound", "The specified product was not found.");
        public static readonly Error BarcodeConflict = new("Product.BarcodeConflict", "A product with this barcode already exists.");
    }

    public static class StockMovement
    {
        public static readonly Error InvalidQuantity = new("StockMovement.InvalidQuantity", "Quantity must be greater than zero.");
        public static readonly Error InvalidReason = new("StockMovement.InvalidReason", "Reason cannot be empty.");
    }

    public static class ProductBalance
    {
        public static readonly Error InsufficientFunds = new("ProductBalance.InsufficientFunds", "Insufficient stock balance for this operation.");
    }
}
