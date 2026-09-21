using Core.Domain;
using Fiscal.Application.Abstractions;
using Fiscal.Application.Abstractions.Models;

namespace Fiscal.Infrastructure.Gateways;

/// <summary>Zeus NFC-e adapter; delegates to fake until homologation wiring is complete.</summary>
public sealed class ZeusNfceGateway : INfceGateway
{
    private readonly FakeNfceGateway _fake = new();

    public Task<Result<NfceTransmitResult>> TransmitAsync(NfceTransmitRequest request, CancellationToken cancellationToken = default) =>
        _fake.TransmitAsync(request, cancellationToken);

    public Task<Result<NfceStatusResult>> QueryStatusAsync(NfceStatusRequest request, CancellationToken cancellationToken = default) =>
        _fake.QueryStatusAsync(request, cancellationToken);

    public Task<Result<NfceCancelResult>> CancelAsync(NfceCancelRequest request, CancellationToken cancellationToken = default) =>
        _fake.CancelAsync(request, cancellationToken);
}
