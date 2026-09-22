using Core.Domain;
using System.Text.RegularExpressions;

namespace Platform.Domain.ValueObjects;

/// <summary>
/// Brazilian CNPJ for platform branch registry (14 digits, checksum not enforced in 9.2).
/// </summary>
public sealed partial record BranchCnpj
{
    /// <summary>Normalized digits-only value.</summary>
    public string Value { get; }

    private BranchCnpj(string value) => Value = value;

    /// <summary>Validates CNPJ format.</summary>
    public static Result<BranchCnpj> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Result.Failure<BranchCnpj>(ErrorCodes.Branch.InvalidCnpj);
        }

        var digits = new string(raw.Where(char.IsDigit).ToArray());
        if (!CnpjDigits().IsMatch(digits))
        {
            return Result.Failure<BranchCnpj>(ErrorCodes.Branch.InvalidCnpj);
        }

        return Result.Success(new BranchCnpj(digits));
    }

    [GeneratedRegex(@"^\d{14}$")]
    private static partial Regex CnpjDigits();
}
