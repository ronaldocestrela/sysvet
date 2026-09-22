using System.Security.Cryptography;
using System.Text;
using Automations.Application.Abstractions;
using Automations.Infrastructure.Configuration;
using ErrorCodes = Automations.Domain.ErrorCodes;
using Core.Domain;
using Microsoft.Extensions.Options;

namespace Automations.Infrastructure.Nps;

/// <summary>
/// HMAC-signed opaque tokens for public NPS survey links.
/// </summary>
public sealed class NpsSurveyTokenService : INpsSurveyTokenService
{
    private readonly AutomationsOptions _options;

    public NpsSurveyTokenService(IOptions<AutomationsOptions> options) => _options = options.Value;

    /// <inheritdoc />
    public string CreateToken(Guid tenantId, Guid inviteId, DateTimeOffset expiresAt)
    {
        var payload = $"{inviteId:N}|{tenantId:N}|{expiresAt.ToUnixTimeSeconds()}";
        var signature = ComputeHmac(payload);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{payload}|{signature}"))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    /// <inheritdoc />
    public Result<(Guid TenantId, Guid InviteId)> Validate(string token, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Result.Failure<(Guid, Guid)>(ErrorCodes.Nps.InvalidToken);
        }

        try
        {
            var padded = token.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }

            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(padded));
            var parts = decoded.Split('|');
            if (parts.Length != 4)
            {
                return Result.Failure<(Guid, Guid)>(ErrorCodes.Nps.InvalidToken);
            }

            if (!Guid.TryParse(parts[0], out var inviteId) || !Guid.TryParse(parts[1], out var tenantId))
            {
                return Result.Failure<(Guid, Guid)>(ErrorCodes.Nps.InvalidToken);
            }

            if (!long.TryParse(parts[2], out var expUnix))
            {
                return Result.Failure<(Guid, Guid)>(ErrorCodes.Nps.InvalidToken);
            }

            var expectedSig = ComputeHmac($"{parts[0]}|{parts[1]}|{parts[2]}");
            if (!CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(expectedSig),
                    Encoding.UTF8.GetBytes(parts[3])))
            {
                return Result.Failure<(Guid, Guid)>(ErrorCodes.Nps.InvalidToken);
            }

            if (now.ToUnixTimeSeconds() > expUnix)
            {
                return Result.Failure<(Guid, Guid)>(ErrorCodes.Nps.Expired);
            }

            return Result.Success((tenantId, inviteId));
        }
        catch (FormatException)
        {
            return Result.Failure<(Guid, Guid)>(ErrorCodes.Nps.InvalidToken);
        }
    }

    /// <inheritdoc />
    public string ComputeTokenHash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    private string ComputeHmac(string payload)
    {
        var key = Encoding.UTF8.GetBytes(_options.NpsTokenSigningKey);
        using var hmac = new HMACSHA256(key);
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
    }
}
