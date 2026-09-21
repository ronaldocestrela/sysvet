namespace Fiscal.Domain.Services;

/// <summary>Maps sales line kinds to NF-e vs NFS-e buckets.</summary>
public static class OrderFiscalLineKind
{
    /// <summary>Product and kit lines contribute to NF-e model 55.</summary>
    public static bool IsNfeLine(string kind) =>
        kind.Equals("Product", StringComparison.OrdinalIgnoreCase)
        || kind.Equals("Kit", StringComparison.OrdinalIgnoreCase);

    /// <summary>Product and kit lines contribute to NFC-e at PDV.</summary>
    public static bool IsNfceLine(string kind) => IsNfeLine(kind);

    /// <summary>Service and package lines contribute to NFS-e Nacional.</summary>
    public static bool IsNfseLine(string kind) =>
        kind.Equals("Service", StringComparison.OrdinalIgnoreCase)
        || kind.Equals("Package", StringComparison.OrdinalIgnoreCase);
}
