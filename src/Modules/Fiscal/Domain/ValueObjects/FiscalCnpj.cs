using Core.Domain;
using System.Text.RegularExpressions;

namespace Fiscal.Domain.ValueObjects;

/// <summary>Issuer or recipient CNPJ (14 digits).</summary>
public sealed partial record FiscalCnpj
{
    public string Value { get; }

    private FiscalCnpj(string value) => Value = value;

    /// <summary>Normalizes and validates CNPJ length.</summary>
    public static Result<FiscalCnpj> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Result.Failure<FiscalCnpj>(ErrorCodes.Cnpj.Invalid);
        }

        var digits = new string(raw.Where(char.IsDigit).ToArray());
        if (!CnpjDigits().IsMatch(digits))
        {
            return Result.Failure<FiscalCnpj>(ErrorCodes.Cnpj.Invalid);
        }

        return Result.Success(new FiscalCnpj(digits));
    }

    [GeneratedRegex(@"^\d{14}$")]
    private static partial Regex CnpjDigits();
}
