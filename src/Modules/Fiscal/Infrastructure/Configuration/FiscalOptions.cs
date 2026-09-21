namespace Fiscal.Infrastructure.Configuration;

/// <summary>Fiscal module configuration.</summary>
public class FiscalOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Fiscal";

    /// <summary>Optional database connection override.</summary>
    public string? ConnectionString { get; set; }

    /// <summary>Fake (default) or ZeusOpenAc.</summary>
    public string Provider { get; set; } = "Fake";

    /// <summary>AES key for certificate password encryption (min 32 chars).</summary>
    public string CertificateEncryptionKey { get; set; } = "dev-fiscal-cert-key-min-32-chars!!";
}
