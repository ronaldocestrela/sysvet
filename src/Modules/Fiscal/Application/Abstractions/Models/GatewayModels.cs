using Fiscal.Domain.Entities;
using Fiscal.Domain.Enums;

namespace Fiscal.Application.Abstractions.Models;

/// <summary>NF-e authorization payload.</summary>
public sealed class NfeAuthorizationRequest
{
    public IssuerProfile Issuer { get; init; } = null!;
    public FiscalDocument Document { get; init; } = null!;
    public byte[] CertificatePfx { get; init; } = Array.Empty<byte>();
    public string CertificatePassword { get; init; } = string.Empty;
    public string RecipientName { get; init; } = string.Empty;
    public string? RecipientCpf { get; init; }
    public string? RecipientStreet { get; init; }
    public string? RecipientNumber { get; init; }
    public string? RecipientDistrict { get; init; }
    public string? RecipientCity { get; init; }
    public string? RecipientState { get; init; }
    public string? RecipientPostalCode { get; init; }
    public int? RecipientIbge { get; init; }
}

/// <summary>NF-e authorization outcome.</summary>
public sealed class NfeAuthorizationResult
{
    public bool Success { get; init; }
    public string? AccessKey { get; init; }
    public string? Protocol { get; init; }
    public string? Xml { get; init; }
    public string? RejectionReason { get; init; }
    public int Number { get; init; }
    public int Series { get; init; }
}

public sealed class NfeCancelRequest
{
    public IssuerProfile Issuer { get; init; } = null!;
    public FiscalDocument Document { get; init; } = null!;
    public byte[] CertificatePfx { get; init; } = Array.Empty<byte>();
    public string CertificatePassword { get; init; } = string.Empty;
    public string Justification { get; init; } = string.Empty;
}

public sealed class NfeCancelResult
{
    public bool Success { get; init; }
    public string? Protocol { get; init; }
    public string? RejectionReason { get; init; }
}

public sealed class NfeCorrectionRequest
{
    public IssuerProfile Issuer { get; init; } = null!;
    public FiscalDocument Document { get; init; } = null!;
    public FiscalCorrectionLetter Letter { get; init; } = null!;
    public byte[] CertificatePfx { get; init; } = Array.Empty<byte>();
    public string CertificatePassword { get; init; } = string.Empty;
}

public sealed class NfeCorrectionResult
{
    public bool Success { get; init; }
    public string? Protocol { get; init; }
    public string? RejectionReason { get; init; }
}

/// <summary>NFS-e Nacional authorization payload.</summary>
public sealed class NfseAuthorizationRequest
{
    public Guid TenantId { get; init; }
    public IssuerProfile Issuer { get; init; } = null!;
    public FiscalDocument Document { get; init; } = null!;
}

public sealed class NfseAuthorizationResult
{
    public bool Success { get; init; }
    public string? NfseNumber { get; init; }
    public string? AccessKey { get; init; }
    public string? Xml { get; init; }
    public string? RejectionReason { get; init; }
}

public sealed class NfseCancelRequest
{
    public Guid TenantId { get; init; }
    public IssuerProfile Issuer { get; init; } = null!;
    public FiscalDocument Document { get; init; } = null!;
    public string Justification { get; init; } = string.Empty;
}

public sealed class NfseCancelResult
{
    public bool Success { get; init; }
    public string? Protocol { get; init; }
    public string? RejectionReason { get; init; }
}

/// <summary>Fiscal provider mode.</summary>
public enum FiscalProviderMode
{
    Fake = 0,
    ZeusOpenAc = 1
}

/// <summary>NFC-e transmission payload.</summary>
public sealed class NfceTransmitRequest
{
    public IssuerProfile Issuer { get; init; } = null!;
    public FiscalDocument Document { get; init; } = null!;
    public string SignedXml { get; init; } = string.Empty;
    public byte[] CertificatePfx { get; init; } = Array.Empty<byte>();
    public string CertificatePassword { get; init; } = string.Empty;
}

public sealed class NfceTransmitResult
{
    public bool Success { get; init; }
    public string? Protocol { get; init; }
    public string? AuthorizedXml { get; init; }
    public string? RejectionReason { get; init; }
}

public sealed class NfceStatusRequest
{
    public IssuerProfile Issuer { get; init; } = null!;
    public string AccessKey { get; init; } = string.Empty;
    public byte[] CertificatePfx { get; init; } = Array.Empty<byte>();
    public string CertificatePassword { get; init; } = string.Empty;
}

public sealed class NfceStatusResult
{
    public bool Authorized { get; init; }
    public string? Protocol { get; init; }
    public string? RejectionReason { get; init; }
}

public sealed class NfceCancelRequest
{
    public IssuerProfile Issuer { get; init; } = null!;
    public FiscalDocument Document { get; init; } = null!;
    public byte[] CertificatePfx { get; init; } = Array.Empty<byte>();
    public string CertificatePassword { get; init; } = string.Empty;
    public string Justification { get; init; } = string.Empty;
}

public sealed class NfceCancelResult
{
    public bool Success { get; init; }
    public string? Protocol { get; init; }
    public string? RejectionReason { get; init; }
}
