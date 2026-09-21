namespace Fiscal.Application.Abstractions;

/// <summary>Encrypts/decrypts A1 certificate secrets at rest.</summary>
public interface ICertificateProtector
{
    /// <summary>Encrypts plain text (password).</summary>
    string Encrypt(string plainText);

    /// <summary>Decrypts cipher text.</summary>
    string Decrypt(string cipherText);
}
