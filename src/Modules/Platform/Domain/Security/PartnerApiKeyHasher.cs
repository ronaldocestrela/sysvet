using System.Security.Cryptography;
using System.Text;

namespace Platform.Domain.Security;

/// <summary>Deterministic SHA-256 hashing for partner API key secrets (9.7).</summary>
public static class PartnerApiKeyHasher
{
    /// <summary>Hashes the raw API key secret for storage and lookup.</summary>
    public static string HashSecret(string secret)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
        return Convert.ToHexString(bytes);
    }
}
