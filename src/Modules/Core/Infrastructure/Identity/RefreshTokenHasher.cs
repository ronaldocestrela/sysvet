using System.Security.Cryptography;
using System.Text;

namespace Core.Infrastructure.Identity;

/// <summary>
/// Computes stable hashes for refresh token storage.
/// </summary>
public static class RefreshTokenHasher
{
    /// <summary>
    /// Returns a Base64 SHA-256 hash of the plain refresh token.
    /// </summary>
    public static string Hash(string plainToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainToken));
        return Convert.ToBase64String(bytes);
    }
}
