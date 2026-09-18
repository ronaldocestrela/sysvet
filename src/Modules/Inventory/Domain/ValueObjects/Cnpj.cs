using Core.Domain;
using Inventory.Domain;
using System.Text.RegularExpressions;

namespace Inventory.Domain.ValueObjects;

/// <summary>
/// Brazilian CNPJ document for suppliers (digits only, 14 characters).
/// </summary>
public sealed partial record Cnpj
{
    public string Value { get; }

    private Cnpj(string value) => Value = value;

    /// <summary>
    /// Validates CNPJ format (14 digits; checksum not enforced in 5.1).
    /// </summary>
    public static Result<Cnpj> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Result.Failure<Cnpj>(ErrorCodes.Supplier.InvalidDocument);
        }

        var digits = new string(raw.Where(char.IsDigit).ToArray());
        if (!CnpjDigits().IsMatch(digits))
        {
            return Result.Failure<Cnpj>(ErrorCodes.Supplier.InvalidDocument);
        }

        return Result.Success(new Cnpj(digits));
    }

    [GeneratedRegex(@"^\d{14}$")]
    private static partial Regex CnpjDigits();
}
