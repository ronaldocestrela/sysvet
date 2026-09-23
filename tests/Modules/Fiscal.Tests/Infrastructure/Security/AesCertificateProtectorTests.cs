using Fiscal.Infrastructure.Configuration;
using Fiscal.Infrastructure.Security;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace Fiscal.Tests.Infrastructure.Security;

public class AesCertificateProtectorTests
{
    [Fact]
    public void EncryptDecrypt_ShouldRoundTrip_WithVersionPrefix()
    {
        var protector = CreateProtector("dev-fiscal-cert-key-min-32-chars!!", null);
        var cipher = protector.Encrypt("secret-password");
        cipher.Should().StartWith("v1:");

        protector.Decrypt(cipher).Should().Be("secret-password");
    }

    [Fact]
    public void Decrypt_ShouldReadLegacyCipher_WithPreviousKeyAfterRotation()
    {
        var oldKey = "dev-fiscal-cert-key-min-32-chars!!";
        var newKey = "rotated-fiscal-cert-key-min-32-chars!";
        var legacyCipher = EncryptLegacy("rotate-me", oldKey);
        legacyCipher.Should().NotStartWith("v1:");

        var rotatedProtector = CreateProtector(newKey, oldKey);
        rotatedProtector.Decrypt(legacyCipher).Should().Be("rotate-me");
    }

    private static AesCertificateProtector CreateProtector(string current, string? previous)
    {
        var options = Options.Create(new FiscalOptions
        {
            CertificateEncryptionKey = current,
            CertificateEncryptionKeyPrevious = previous
        });
        return new AesCertificateProtector(options);
    }

    private static string EncryptLegacy(string plainText, string keyMaterial)
    {
        using var aes = System.Security.Cryptography.Aes.Create();
        aes.Key = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(keyMaterial));
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor();
        var plainBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        var cipher = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        var payload = aes.IV.Concat(cipher).ToArray();
        return Convert.ToBase64String(payload);
    }
}
