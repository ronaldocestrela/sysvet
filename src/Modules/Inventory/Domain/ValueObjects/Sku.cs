using Core.Domain;
using Inventory.Domain;

namespace Inventory.Domain.ValueObjects;

/// <summary>
/// Stock keeping unit identifier, unique per tenant.
/// </summary>
public sealed record Sku
{
    public string Value { get; }

    private Sku(string value) => Value = value;

    /// <summary>
    /// Validates and creates a SKU (1–40 alphanumeric/dash characters).
    /// </summary>
    public static Result<Sku> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Result.Failure<Sku>(ErrorCodes.Product.InvalidSku);
        }

        var normalized = raw.Trim().ToUpperInvariant();
        if (normalized.Length is < 1 or > 40)
        {
            return Result.Failure<Sku>(ErrorCodes.Product.InvalidSku);
        }

        return Result.Success(new Sku(normalized));
    }

    public override string ToString() => Value;
}
