using Fiscal.Application.Abstractions;
using Microsoft.Extensions.Options;
using Fiscal.Infrastructure.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Fiscal.Infrastructure.Security;

/// <summary>AES protector for certificate passwords at rest.</summary>
public sealed class AesCertificateProtector : ICertificateProtector
{
    private readonly byte[] _key;

    public AesCertificateProtector(IOptions<FiscalOptions> options)
    {
        var material = options.Value.CertificateEncryptionKey ?? string.Empty;
        _key = SHA256.HashData(Encoding.UTF8.GetBytes(material));
    }

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipher = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        var payload = aes.IV.Concat(cipher).ToArray();
        return Convert.ToBase64String(payload);
    }

    public string Decrypt(string cipherText)
    {
        var payload = Convert.FromBase64String(cipherText);
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = payload.Take(16).ToArray();
        using var decryptor = aes.CreateDecryptor();
        var cipher = payload.Skip(16).ToArray();
        var plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
        return Encoding.UTF8.GetString(plain);
    }
}
