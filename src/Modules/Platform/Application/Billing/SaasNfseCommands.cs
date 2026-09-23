using Core.Application.Messaging;
using Core.Domain;

namespace Platform.Application.Billing;

/// <summary>Emits or retries VetNexus NFS-e for a settled billing invoice (9.6).</summary>
/// <param name="InvoiceId">Platform billing invoice id.</param>
/// <param name="ForceRetry">When true, retries failed rows.</param>
public sealed record IssueSaasNfseForInvoiceCommand(Guid InvoiceId, bool ForceRetry = false) : ICommand;

/// <summary>Super Admin retry wrapper with tenant ownership check.</summary>
public sealed record RetrySaasNfseForInvoiceCommand(Guid TenantId, Guid InvoiceId) : ICommand<SaasNfseDto>;

/// <summary>NFS-e row API view.</summary>
public sealed record SaasNfseDto(
    Guid Id,
    Guid BillingInvoiceId,
    Guid TenantId,
    decimal Amount,
    SaasServiceInvoiceStatusDto Status,
    string? NfseNumber,
    string? AccessKey,
    string? FailureReason);

/// <summary>API enum mirror.</summary>
public enum SaasServiceInvoiceStatusDto
{
    Pending = 0,
    Authorized = 1,
    Failed = 2
}
