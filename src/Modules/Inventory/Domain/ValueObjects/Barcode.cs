using Core.Domain;
using Inventory.Domain;

namespace Inventory.Domain.ValueObjects;

/// <summary>
/// Product barcode (EAN/GTIN or internal), unique per tenant.
/// </summary>
public sealed record Barcode
{
    public string Value { get; }

    private Barcode(string value) => Value = value;

    /// <summary>
    /// Validates and creates a barcode (4–50 characters).
    /// </summary>
    public static Result<Barcode> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Result.Failure<Barcode>(ErrorCodes.Product.InvalidBarcode);
        }

        var normalized = raw.Trim();
        if (normalized.Length is < 4 or > 50)
        {
            return Result.Failure<Barcode>(ErrorCodes.Product.InvalidBarcode);
        }

        return Result.Success(new Barcode(normalized));
    }

    public override string ToString() => Value;
}
