namespace Platform.Application.Abstractions;

/// <summary>Generates one-time partner API key secrets (9.7).</summary>
public interface IPartnerApiKeySecretGenerator
{
    /// <summary>Creates a new secret prefixed with vn_.</summary>
    string GenerateSecret();
}
