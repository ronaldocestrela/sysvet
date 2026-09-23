using Fiscal.Application.Abstractions;
using Microsoft.Extensions.Options;
using Fiscal.Infrastructure.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Fiscal.Infrastructure.Security;

/// <summary>AES protector for certificate passwords at rest with optional key rotation.</summary>
public sealed class AesCertificateProtector : ICertificateProtector
{
    private const string VersionPrefix = "v1:";
    private readonly byte[] _currentKey;
    private readonly byte[]? _previousKey;

    /// <summary>Initializes keys from fiscal options.</summary>
    public AesCertificateProtector(IOptions<FiscalOptions> options)
    {
        var fiscal = options.Value;
        _currentKey = DeriveKey(fiscal.CertificateEncryptionKey);
        _previousKey = string.IsNullOrWhiteSpace(fiscal.CertificateEncryptionKeyPrevious)
            ? null
            : DeriveKey(fiscal.CertificateEncryptionKeyPrevious!);
    }

    /// <inheritdoc />
    public string Encrypt(string plainText)
    {
        var cipher = EncryptWithKey(plainText, _currentKey);
        return VersionPrefix + cipher;
    }

    /// <inheritdoc />
    public string Decrypt(string cipherText)
    {
        if (cipherText.StartsWith(VersionPrefix, StringComparison.Ordinal))
        {
            return DecryptWithKey(cipherText[VersionPrefix.Length..], _currentKey);
        }

        try
        {
            return DecryptWithKey(cipherText, _currentKey);
        }
        catch (CryptographicException) when (_previousKey is not null)
        {
            return DecryptWithKey(cipherText, _previousKey);
        }
    }

    private static byte[] DeriveKey(string material)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(material ?? string.Empty));
    }

    private static string EncryptWithKey(string plainText, byte[] key)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipher = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        var payload = aes.IV.Concat(cipher).ToArray();
        return Convert.ToBase64String(payload);
    }

    private static string DecryptWithKey(string cipherText, byte[] key)
    {
        var payload = Convert.FromBase64String(cipherText);
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = payload.Take(16).ToArray();
        using var decryptor = aes.CreateDecryptor();
        var cipher = payload.Skip(16).ToArray();
        var plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
        return Encoding.UTF8.GetString(plain);
    }
}
