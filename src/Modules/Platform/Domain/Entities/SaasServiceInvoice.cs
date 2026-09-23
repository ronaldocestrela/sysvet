using Core.Domain;

namespace Platform.Domain.Entities;

/// <summary>
/// NFS-e issued by VetNexus against a tenant for a settled SaaS billing invoice (dbo; ADR-051).
/// </summary>
public sealed class SaasServiceInvoice : Entity
{
    /// <summary>Source platform invoice.</summary>
    public Guid BillingInvoiceId { get; private set; }

    /// <summary>Owning tenant (clinic).</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Service amount in BRL (mirrors invoice total).</summary>
    public decimal Amount { get; private set; }

    /// <summary>Recipient CNPJ digits.</summary>
    public string RecipientCnpj { get; private set; } = string.Empty;

    /// <summary>Recipient legal name.</summary>
    public string RecipientLegalName { get; private set; } = string.Empty;

    /// <summary>Emission status.</summary>
    public SaasServiceInvoiceStatus Status { get; private set; }

    /// <summary>Authorized NFS-e number when successful.</summary>
    public string? NfseNumber { get; private set; }

    /// <summary>Access key when authorized.</summary>
    public string? AccessKey { get; private set; }

    /// <summary>Blob key for authorized XML.</summary>
    public string? XmlBlobKey { get; private set; }

    /// <summary>Failure reason when status is failed.</summary>
    public string? FailureReason { get; private set; }

#pragma warning disable CS8618
    private SaasServiceInvoice()
    {
    }
#pragma warning restore CS8618

    /// <summary>Creates a pending NFS-e row for a paid invoice.</summary>
    public static Result<SaasServiceInvoice> CreatePending(
        Guid billingInvoiceId,
        Guid tenantId,
        decimal amount,
        string recipientCnpj,
        string recipientLegalName)
    {
        if (billingInvoiceId == Guid.Empty || tenantId == Guid.Empty)
        {
            return Result.Failure<SaasServiceInvoice>(ErrorCodes.Billing.InvalidTenant);
        }

        if (amount <= 0)
        {
            return Result.Failure<SaasServiceInvoice>(ErrorCodes.Nfse.InvalidAmount);
        }

        var cnpj = new string((recipientCnpj ?? string.Empty).Where(char.IsDigit).ToArray());
        if (cnpj.Length != 14)
        {
            return Result.Failure<SaasServiceInvoice>(ErrorCodes.Nfse.InvalidRecipient);
        }

        var name = recipientLegalName?.Trim() ?? string.Empty;
        if (name.Length < 2)
        {
            return Result.Failure<SaasServiceInvoice>(ErrorCodes.Nfse.InvalidRecipient);
        }

        return Result.Success(new SaasServiceInvoice
        {
            Id = Guid.NewGuid(),
            BillingInvoiceId = billingInvoiceId,
            TenantId = tenantId,
            Amount = amount,
            RecipientCnpj = cnpj,
            RecipientLegalName = name,
            Status = SaasServiceInvoiceStatus.Pending,
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = new byte[8]
        });
    }

    /// <summary>Marks the NFS-e as authorized.</summary>
    public Result MarkAuthorized(string nfseNumber, string accessKey, string xmlBlobKey)
    {
        if (Status == SaasServiceInvoiceStatus.Authorized)
        {
            return Result.Success();
        }

        if (Status != SaasServiceInvoiceStatus.Pending && Status != SaasServiceInvoiceStatus.Failed)
        {
            return Result.Failure(ErrorCodes.Nfse.InvalidTransition);
        }

        if (string.IsNullOrWhiteSpace(nfseNumber) || string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(xmlBlobKey))
        {
            return Result.Failure(ErrorCodes.Nfse.InvalidGatewayResponse);
        }

        Status = SaasServiceInvoiceStatus.Authorized;
        NfseNumber = nfseNumber.Trim();
        AccessKey = accessKey.Trim();
        XmlBlobKey = xmlBlobKey.Trim();
        FailureReason = null;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Marks emission failed without affecting billing settlement.</summary>
    public Result MarkFailed(string reason)
    {
        if (Status == SaasServiceInvoiceStatus.Authorized)
        {
            return Result.Failure(ErrorCodes.Nfse.InvalidTransition);
        }

        Status = SaasServiceInvoiceStatus.Failed;
        FailureReason = string.IsNullOrWhiteSpace(reason) ? "Emission failed." : reason.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Resets a failed row to pending for manual retry.</summary>
    public Result ResetForRetry()
    {
        if (Status == SaasServiceInvoiceStatus.Authorized)
        {
            return Result.Failure(ErrorCodes.Nfse.AlreadyAuthorized);
        }

        Status = SaasServiceInvoiceStatus.Pending;
        FailureReason = null;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
