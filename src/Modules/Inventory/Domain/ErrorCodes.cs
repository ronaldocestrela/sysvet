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
        public static readonly Error InvalidUnitsPerPackage = new("Product.InvalidUnitsPerPackage", "Units per package must be greater than zero.");
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
        public static readonly Error Inactive = new("ProductLot.Inactive", "The lot is inactive.");
    }

    public static class StockMovement
    {
        public static readonly Error InvalidQuantity = new("StockMovement.InvalidQuantity", "Quantity must be greater than zero.");
        public static readonly Error InvalidReason = new("StockMovement.InvalidReason", "Reason cannot be empty.");
        public static readonly Error InvalidType = new("StockMovement.InvalidType", "Movement type is not supported.");
        public static readonly Error LotRequired = new("StockMovement.LotRequired", "Product lot is required for this product.");
        public static readonly Error LotProductMismatch = new("StockMovement.LotProductMismatch", "Lot does not belong to the product.");
        public static readonly Error TransferSameLot = new("StockMovement.TransferSameLot", "Source and destination lots must differ.");
        public static readonly Error TransferProductMismatch = new("StockMovement.TransferProductMismatch", "Lots must belong to the same product.");
        public static readonly Error InvalidAdjustmentDirection = new("StockMovement.InvalidAdjustmentDirection", "Adjustment direction is required for adjustment movements.");
        public static readonly Error InvalidLossReason = new("StockMovement.InvalidLossReason", "Loss reason is not allowed.");
        public static readonly Error FractionationRequiresLot = new("StockMovement.FractionationRequiresLot", "Fractionation requires a sealed product lot.");
        public static readonly Error CannotFractionateOpenLot = new("StockMovement.CannotFractionateOpenLot", "Cannot fractionate an already fractional lot.");
        public static readonly Error SupplierRequired = new("StockMovement.SupplierRequired", "Supplier is required for supplier returns.");
    }

    public static class ProductBalance
    {
        public static readonly Error InsufficientFunds = new("ProductBalance.InsufficientFunds", "Insufficient stock balance for this operation.");
    }

    public static class Money
    {
        public static readonly Error InvalidAmount = new("Money.InvalidAmount", "Amount cannot be negative.");
    }

    public static class PurchaseImport
    {
        public static readonly Error InvalidXml = new("PurchaseImport.InvalidXml", "The NF-e XML could not be parsed.");
        public static readonly Error InvalidAccessKey = new("PurchaseImport.InvalidAccessKey", "Access key must contain 44 digits.");
        public static readonly Error AccessKeyConflict = new("PurchaseImport.AccessKeyConflict", "This NF-e was already imported and confirmed.");
        public static readonly Error NotFound = new("PurchaseImport.NotFound", "Purchase import was not found.");
        public static readonly Error AlreadyConfirmed = new("PurchaseImport.AlreadyConfirmed", "Import is already confirmed.");
        public static readonly Error NoLines = new("PurchaseImport.NoLines", "NF-e has no product lines.");
        public static readonly Error MissingBlobKey = new("PurchaseImport.MissingBlobKey", "Stored XML key is required.");
        public static readonly Error SupplierRequired = new("PurchaseImport.SupplierRequired", "Supplier must be linked before confirm.");
        public static readonly Error LineWithoutProduct = new("PurchaseImport.LineWithoutProduct", "All lines must be mapped to products.");
        public static readonly Error StockNotApplied = new("PurchaseImport.StockNotApplied", "Stock movements were not applied to all lines.");
        public static readonly Error InvalidSupplierProductCode = new("PurchaseImport.InvalidSupplierProductCode", "Supplier product code is required.");
        public static readonly Error FileTooLarge = new("PurchaseImport.FileTooLarge", "XML file exceeds the maximum allowed size.");
        public static readonly Error InvalidContentType = new("PurchaseImport.InvalidContentType", "Upload must be an XML file.");
        public static readonly Error LineNotFound = new("PurchaseImport.LineNotFound", "Import line was not found.");
    }

    public static class InventoryCount
    {
        public static readonly Error NotFound = new("InventoryCount.NotFound", "Inventory count session was not found.");
        public static readonly Error AlreadyInProgress = new("InventoryCount.AlreadyInProgress", "Another inventory count is already in progress.");
        public static readonly Error InvalidStatus = new("InventoryCount.InvalidStatus", "Operation is not allowed in the current session status.");
        public static readonly Error EmptyLines = new("InventoryCount.EmptyLines", "At least one count line is required.");
        public static readonly Error InvalidQuantity = new("InventoryCount.InvalidQuantity", "Quantity must be greater than zero.");
        public static readonly Error LineNotFound = new("InventoryCount.LineNotFound", "Count line was not found.");
        public static readonly Error LotRequired = new("InventoryCount.LotRequired", "Product lot is required for this product.");
        public static readonly Error InvalidCode = new("InventoryCount.InvalidCode", "Session code is invalid.");
        public static readonly Error NotSubmitted = new("InventoryCount.NotSubmitted", "Session must be submitted before approval.");
    }
}
