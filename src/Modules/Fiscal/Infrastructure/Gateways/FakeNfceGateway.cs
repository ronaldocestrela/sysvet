using Core.Domain;
using Fiscal.Application.Abstractions;
using Fiscal.Application.Abstractions.Models;

namespace Fiscal.Infrastructure.Gateways;

/// <summary>Deterministic NFC-e gateway for CI and development.</summary>
public sealed class FakeNfceGateway : INfceGateway
{
    public Task<Result<NfceTransmitResult>> TransmitAsync(NfceTransmitRequest request, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(new NfceTransmitResult
        {
            Success = true,
            Protocol = "135650000000000",
            AuthorizedXml = request.SignedXml
        }));

    public Task<Result<NfceStatusResult>> QueryStatusAsync(NfceStatusRequest request, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(new NfceStatusResult
        {
            Authorized = true,
            Protocol = "135650000000000"
        }));

    public Task<Result<NfceCancelResult>> CancelAsync(NfceCancelRequest request, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(new NfceCancelResult { Success = true, Protocol = "135650000000001" }));
}
