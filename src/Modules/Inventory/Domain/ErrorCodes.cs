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
        public static readonly Error InvalidSku = new("Product.InvalidSku", "SKU is invalid.");
        public static readonly Error InvalidBarcode = new("Product.InvalidBarcode", "Product barcode is invalid.");
        public static readonly Error InvalidReorderLevel = new("Product.InvalidReorderLevel", "Reorder level cannot be negative.");
        public static readonly Error InvalidNcm = new("Product.InvalidNcm", "NCM must be eight digits.");
        public static readonly Error InvalidMerchandiseOrigin = new("Product.InvalidMerchandiseOrigin", "Merchandise origin must be between 0 and 8.");
        public static readonly Error NotFound = new("Product.NotFound", "The specified product was not found.");
        public static readonly Error BarcodeConflict = new("Product.BarcodeConflict", "A product with this barcode already exists.");
        public static readonly Error SkuConflict = new("Product.SkuConflict", "A product with this SKU already exists.");
        public static readonly Error Inactive = new("Product.Inactive", "The product is inactive.");
    }

    public static class Supplier
    {
        public static readonly Error InvalidLegalName = new("Supplier.InvalidLegalName", "Supplier legal name cannot be empty.");
        public static readonly Error InvalidDocument = new("Supplier.InvalidDocument", "Supplier CNPJ is invalid.");
        public static readonly Error NotFound = new("Supplier.NotFound", "The specified supplier was not found.");
        public static readonly Error DocumentConflict = new("Supplier.DocumentConflict", "A supplier with this CNPJ already exists.");
        public static readonly Error Inactive = new("Supplier.Inactive", "The supplier is inactive.");
    }

    public static class ProductLot
    {
        public static readonly Error InvalidProduct = new("ProductLot.InvalidProduct", "Product id is required.");
        public static readonly Error InvalidLotNumber = new("ProductLot.InvalidLotNumber", "Lot number is required.");
        public static readonly Error InvalidUnitCost = new("ProductLot.InvalidUnitCost", "Unit cost cannot be negative.");
        public static readonly Error InvalidQuantity = new("ProductLot.InvalidQuantity", "Quantity cannot be negative.");
        public static readonly Error InsufficientQuantity = new("ProductLot.InsufficientQuantity", "Insufficient lot quantity.");
        public static readonly Error NotFound = new("ProductLot.NotFound", "The specified lot was not found.");
        public static readonly Error LotNumberConflict = new("ProductLot.LotNumberConflict", "This lot number already exists for the product.");
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

    public static class Money
    {
        public static readonly Error InvalidAmount = new("Money.InvalidAmount", "Amount cannot be negative.");
    }
}
