using System.Text.RegularExpressions;
using Core.Domain;

namespace Automations.Domain.Services;

/// <summary>
/// Replaces <c>{{TokenName}}</c> placeholders in template bodies using a token dictionary.
/// </summary>
public static partial class MessageTemplateRenderer
{
    [GeneratedRegex(@"\{\{(\w+)\}\}", RegexOptions.Compiled)]
    private static partial Regex TokenPattern();

    /// <summary>
    /// Renders the template body; unknown tokens return a failure result.
    /// </summary>
    public static Result<string> Render(string body, IReadOnlyDictionary<string, string> tokens)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return Result.Failure<string>(ErrorCodes.Template.InvalidBody);
        }

        var matches = TokenPattern().Matches(body);
        foreach (Match match in matches)
        {
            var tokenName = match.Groups[1].Value;
            if (!tokens.TryGetValue(tokenName, out var value))
            {
                return Result.Failure<string>(ErrorCodes.Template.UnknownToken);
            }

            body = body.Replace(match.Value, value, StringComparison.Ordinal);
        }

        return Result.Success(body);
    }
}
