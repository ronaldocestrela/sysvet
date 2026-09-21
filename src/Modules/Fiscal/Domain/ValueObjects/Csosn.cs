using Core.Domain;
using System.Text.RegularExpressions;

namespace Fiscal.Domain.ValueObjects;

/// <summary>CSOSN for Simples Nacional (3 digits).</summary>
public sealed partial record Csosn
{
    public string Value { get; }

    private Csosn(string value) => Value = value;

    /// <summary>Creates a validated CSOSN.</summary>
    public static Result<Csosn> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Result.Failure<Csosn>(new Error("Fiscal.Csosn.Invalid", "CSOSN must have 3 digits."));
        }

        var digits = new string(raw.Where(char.IsDigit).ToArray());
        if (!CsosnDigits().IsMatch(digits))
        {
            return Result.Failure<Csosn>(new Error("Fiscal.Csosn.Invalid", "CSOSN must have 3 digits."));
        }

        return Result.Success(new Csosn(digits));
    }

    [GeneratedRegex(@"^\d{3}$")]
    private static partial Regex CsosnDigits();
}
