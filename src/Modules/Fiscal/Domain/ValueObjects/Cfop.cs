using Core.Domain;
using System.Text.RegularExpressions;

namespace Fiscal.Domain.ValueObjects;

/// <summary>CFOP code (4 digits).</summary>
public sealed partial record Cfop
{
    public string Value { get; }

    private Cfop(string value) => Value = value;

    /// <summary>Creates a validated CFOP.</summary>
    public static Result<Cfop> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Result.Failure<Cfop>(new Error("Fiscal.Cfop.Invalid", "CFOP must have 4 digits."));
        }

        var digits = new string(raw.Where(char.IsDigit).ToArray());
        if (!CfopDigits().IsMatch(digits))
        {
            return Result.Failure<Cfop>(new Error("Fiscal.Cfop.Invalid", "CFOP must have 4 digits."));
        }

        return Result.Success(new Cfop(digits));
    }

    [GeneratedRegex(@"^\d{4}$")]
    private static partial Regex CfopDigits();
}
