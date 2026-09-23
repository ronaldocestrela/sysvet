namespace Platform.Application.Configuration;

/// <summary>VetNexus NFS-e issuer settings (Platform:Nfse).</summary>
public sealed class NfseOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Platform:Nfse";

    /// <summary>Fake (default) or OpenAc.</summary>
    public string Provider { get; set; } = "Fake";

    /// <summary>VetNexus issuer CNPJ digits.</summary>
    public string IssuerCnpj { get; set; } = string.Empty;

    /// <summary>VetNexus issuer IBGE municipality code.</summary>
    public int IssuerIbgeCityCode { get; set; }

    /// <summary>Blob key for VetNexus A1 certificate PFX.</summary>
    public string? CertificateBlobKey { get; set; }

    /// <summary>Encrypted or plain certificate password (dev only).</summary>
    public string? CertificatePassword { get; set; }

    /// <summary>OpenAC environment key (homologation/production).</summary>
    public string Environment { get; set; } = "Homologation";
}
