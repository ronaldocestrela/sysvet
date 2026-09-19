using System.Text;

namespace Inventory.Domain.Services;

/// <summary>
/// Builds ZPL II label payloads for thermal printers (60×40 mm, Code128 barcode).
/// </summary>
public static class ZplLabelEncoder
{
    private const int MaxNameLength = 28;

    /// <summary>
    /// Encodes one or more identical product labels as concatenated ZPL jobs.
    /// </summary>
    public static string Encode(string productName, string sku, string barcode, int copies)
    {
        var count = Math.Clamp(copies, 1, 50);
        var name = Truncate(productName);
        var sb = new StringBuilder();
        for (var i = 0; i < count; i++)
        {
            sb.AppendLine("^XA");
            sb.AppendLine("^PW480");
            sb.AppendLine("^LL320");
            sb.AppendLine("^FO20,20^A0N,28,28^FD").Append(EscapeField(name)).AppendLine("^FS");
            sb.AppendLine("^FO20,55^A0N,22,22^FD").Append(EscapeField($"SKU: {sku}")).AppendLine("^FS");
            sb.AppendLine("^FO20,95^BY2^BCN,80,Y,N,N^FD").Append(EscapeField(barcode)).AppendLine("^FS");
            sb.AppendLine("^XZ");
        }

        return sb.ToString();
    }

    private static string Truncate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= MaxNameLength ? trimmed : trimmed[..MaxNameLength];
    }

    private static string EscapeField(string value) =>
        value.Replace('^', ' ').Replace('~', ' ');
}
