namespace Clients.Infrastructure.Fiscal;

/// <summary>Cached issuer profile and certificate for PDV NFC-e (ADR-036).</summary>
public sealed class OfflineFiscalIssuerCache
{
    public Guid Id { get; set; } = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public string LegalName { get; set; } = string.Empty;
    public string TradeName { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string State { get; set; } = "SP";
    public int IbgeCityCode { get; set; }
    public int NfceSeries { get; set; } = 1;
    public bool HasCertificate { get; set; }
    public string? EncryptedPfxBase64 { get; set; }
    public string? EncryptedCertificatePassword { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>Local NFC-e sequence per device.</summary>
public sealed class OfflineFiscalSequence
{
    public Guid Id { get; set; } = Guid.Parse("00000000-0000-0000-0000-000000000002");
    public int NfceSeries { get; set; } = 1;
    public long NextNfceNumber { get; set; } = 1;
}

/// <summary>Local mirror of NFC-e documents for receipt and offline listing.</summary>
public sealed class OfflineFiscalDocument
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string DocumentType { get; set; } = "Nfce";
    public string Status { get; set; } = string.Empty;
    public string? AccessKey { get; set; }
    public string? Protocol { get; set; }
    public string? QrCodeUrl { get; set; }
    public string? SignedXml { get; set; }
    public int? NfeNumber { get; set; }
    public int? NfeSeries { get; set; }
    public string RecipientName { get; set; } = "Consumidor";
    public string? RecipientCpf { get; set; }
    public string? EmissionType { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? AuthorizedAt { get; set; }
}
