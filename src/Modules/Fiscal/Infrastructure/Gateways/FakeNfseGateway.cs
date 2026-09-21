using Core.Domain;
using Fiscal.Application.Abstractions;
using Fiscal.Application.Abstractions.Models;

namespace Fiscal.Infrastructure.Gateways;

/// <summary>Deterministic NFS-e Nacional gateway for CI and development.</summary>
public sealed class FakeNfseGateway : INfseGateway
{
    public Task<Result<NfseAuthorizationResult>> AuthorizeAsync(NfseAuthorizationRequest request, CancellationToken cancellationToken = default)
    {
        var number = request.Issuer.NextDpsNumber.ToString();
        return Task.FromResult(Result.Success(new NfseAuthorizationResult
        {
            Success = true,
            NfseNumber = number,
            AccessKey = $"NFSE{number.PadLeft(40, '0')}",
            Xml = $"<NFSe><Numero>{number}</Numero></NFSe>"
        }));
    }

    public Task<Result<NfseCancelResult>> CancelAsync(NfseCancelRequest request, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(new NfseCancelResult { Success = true, Protocol = "ADN-CANCEL-1" }));
}
