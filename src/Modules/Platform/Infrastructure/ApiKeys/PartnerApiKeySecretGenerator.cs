using System.Security.Cryptography;
using Platform.Application.Abstractions;

namespace Platform.Infrastructure.ApiKeys;

/// <summary>Generates cryptographically random partner API key secrets (9.7).</summary>
public sealed class PartnerApiKeySecretGenerator : IPartnerApiKeySecretGenerator
{
    /// <inheritdoc />
    public string GenerateSecret()
    {
        Span<byte> bytes = stackalloc byte[24];
        RandomNumberGenerator.Fill(bytes);
        return "vn_" + Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', 'x').Replace('/', 'y');
    }
}
