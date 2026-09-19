using System.Text;

namespace Inventory.Application.PurchaseImports;

/// <summary>
/// Normalizes supplier product codes into SKU candidates.
/// </summary>
public static class PurchaseImportSkuHelper
{
    /// <summary>
    /// Builds a SKU from cProd (alphanumeric and dash, max 40, upper case).
    /// </summary>
    public static string SanitizeSku(string supplierProductCode)
    {
        if (string.IsNullOrWhiteSpace(supplierProductCode))
        {
            return "ITEM";
        }

        var sb = new StringBuilder();
        foreach (var c in supplierProductCode.Trim().ToUpperInvariant())
        {
            if (char.IsLetterOrDigit(c) || c == '-')
            {
                sb.Append(c);
            }
        }

        var sku = sb.ToString();
        if (sku.Length == 0)
        {
            sku = "ITEM";
        }

        return sku.Length <= 40 ? sku : sku[..40];
    }
}
