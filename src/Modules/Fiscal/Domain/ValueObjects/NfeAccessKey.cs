using Core.Domain;
using System.Text.RegularExpressions;

namespace Fiscal.Domain.ValueObjects;

/// <summary>NF-e access key (44 digits).</summary>
public sealed partial record NfeAccessKey
{
    public string Value { get; }

    private NfeAccessKey(string value) => Value = value;

    /// <summary>Validates a 44-digit NF-e key.</summary>
    public static Result<NfeAccessKey> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Result.Failure<NfeAccessKey>(ErrorCodes.AccessKey.Invalid);
        }

        var digits = new string(raw.Where(char.IsDigit).ToArray());
        if (!KeyDigits().IsMatch(digits))
        {
            return Result.Failure<NfeAccessKey>(ErrorCodes.AccessKey.Invalid);
        }

        return Result.Success(new NfeAccessKey(digits));
    }

    [GeneratedRegex(@"^\d{44}$")]
    private static partial Regex KeyDigits();
}
