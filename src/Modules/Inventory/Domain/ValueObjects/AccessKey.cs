using Core.Domain;
using Inventory.Domain;
using System.Text.RegularExpressions;

namespace Inventory.Domain.ValueObjects;

/// <summary>
/// NF-e access key (chNFe) — 44 numeric digits identifying the document.
/// </summary>
public sealed partial record AccessKey
{
    public string Value { get; }

    private AccessKey(string value) => Value = value;

    /// <summary>
    /// Validates and normalizes a 44-digit access key.
    /// </summary>
    public static Result<AccessKey> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Result.Failure<AccessKey>(ErrorCodes.PurchaseImport.InvalidAccessKey);
        }

        var digits = new string(raw.Where(char.IsDigit).ToArray());
        if (!AccessKeyDigits().IsMatch(digits))
        {
            return Result.Failure<AccessKey>(ErrorCodes.PurchaseImport.InvalidAccessKey);
        }

        return Result.Success(new AccessKey(digits));
    }

    [GeneratedRegex(@"^\d{44}$")]
    private static partial Regex AccessKeyDigits();
}
