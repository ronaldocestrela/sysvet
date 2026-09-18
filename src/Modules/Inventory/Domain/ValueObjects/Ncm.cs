using Core.Domain;
using Inventory.Domain;
using System.Text.RegularExpressions;

namespace Inventory.Domain.ValueObjects;

/// <summary>
/// Brazilian NCM merchandise code (8 digits) for future NF-e integration.
/// </summary>
public sealed partial record Ncm
{
    public string Value { get; }

    private Ncm(string value) => Value = value;

    /// <summary>
    /// Validates NCM as exactly eight digits.
    /// </summary>
    public static Result<Ncm> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Result.Failure<Ncm>(ErrorCodes.Product.InvalidNcm);
        }

        var digits = raw.Trim().Replace(".", string.Empty, StringComparison.Ordinal);
        if (!NcmDigits().IsMatch(digits))
        {
            return Result.Failure<Ncm>(ErrorCodes.Product.InvalidNcm);
        }

        return Result.Success(new Ncm(digits));
    }

    [GeneratedRegex(@"^\d{8}$")]
    private static partial Regex NcmDigits();
}
