using Core.Domain;

namespace Platform.Application.Abstractions;

/// <summary>VetNexus NFS-e Nacional gateway port (9.6).</summary>
public interface ISaasNfseGateway
{
    /// <summary>Authorizes NFS-e for a SaaS billing invoice.</summary>
    Task<Result<SaasNfseAuthorizationResult>> AuthorizeAsync(SaasNfseAuthorizationRequest request, CancellationToken cancellationToken = default);
}

/// <summary>Payload to emit NFS-e against a tenant headquarters.</summary>
public sealed class SaasNfseAuthorizationRequest
{
    /// <summary>Billing invoice id (external reference).</summary>
    public Guid BillingInvoiceId { get; init; }

    /// <summary>Service amount in BRL.</summary>
    public decimal Amount { get; init; }

    /// <summary>Recipient CNPJ digits.</summary>
    public string RecipientCnpj { get; init; } = string.Empty;

    /// <summary>Recipient legal name.</summary>
    public string RecipientLegalName { get; init; } = string.Empty;

    /// <summary>Recipient postal code digits.</summary>
    public string? RecipientPostalCode { get; init; }

    /// <summary>Recipient street.</summary>
    public string? RecipientStreet { get; init; }

    /// <summary>Recipient street number.</summary>
    public string? RecipientStreetNumber { get; init; }

    /// <summary>Recipient district.</summary>
    public string? RecipientDistrict { get; init; }

    /// <summary>Recipient city.</summary>
    public string? RecipientCity { get; init; }

    /// <summary>Recipient UF.</summary>
    public string? RecipientStateCode { get; init; }

    /// <summary>Recipient IBGE code.</summary>
    public int? RecipientIbgeCode { get; init; }
}

/// <summary>Gateway authorization outcome.</summary>
public sealed class SaasNfseAuthorizationResult
{
    /// <summary>When true, NFS-e was authorized.</summary>
    public bool Success { get; init; }

    /// <summary>Authorized number.</summary>
    public string? NfseNumber { get; init; }

    /// <summary>Access key.</summary>
    public string? AccessKey { get; init; }

    /// <summary>Authorized XML payload.</summary>
    public string? Xml { get; init; }

    /// <summary>Failure reason when not successful.</summary>
    public string? FailureReason { get; init; }
}
