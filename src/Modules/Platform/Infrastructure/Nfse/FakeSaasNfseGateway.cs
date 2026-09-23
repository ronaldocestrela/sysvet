using Core.Domain;
using Platform.Application.Abstractions;

namespace Platform.Infrastructure.Nfse;

/// <summary>Deterministic VetNexus NFS-e gateway for CI/dev (9.6).</summary>
public sealed class FakeSaasNfseGateway : ISaasNfseGateway
{
    /// <inheritdoc />
    public Task<Result<SaasNfseAuthorizationResult>> AuthorizeAsync(
        SaasNfseAuthorizationRequest request,
        CancellationToken cancellationToken = default)
    {
        var suffix = request.BillingInvoiceId.ToString("N")[..8];
        var number = suffix;
        return Task.FromResult(Result.Success(new SaasNfseAuthorizationResult
        {
            Success = true,
            NfseNumber = number,
            AccessKey = $"SAASNFSE{number.PadLeft(40, '0')}",
            Xml = $"<NFSe><InvoiceId>{request.BillingInvoiceId}</InvoiceId><Valor>{request.Amount}</Valor></NFSe>"
        }));
    }
}
