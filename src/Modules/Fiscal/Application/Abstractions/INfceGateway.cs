using Core.Domain;
using Fiscal.Application.Abstractions.Models;

namespace Fiscal.Application.Abstractions;

/// <summary>SEFAZ NFC-e model 65 transmission port.</summary>
public interface INfceGateway
{
    /// <summary>Transmits a signed NFC-e XML to SEFAZ.</summary>
    Task<Result<NfceTransmitResult>> TransmitAsync(NfceTransmitRequest request, CancellationToken cancellationToken = default);

    /// <summary>Queries authorization status by access key.</summary>
    Task<Result<NfceStatusResult>> QueryStatusAsync(NfceStatusRequest request, CancellationToken cancellationToken = default);

    /// <summary>Cancels an authorized NFC-e.</summary>
    Task<Result<NfceCancelResult>> CancelAsync(NfceCancelRequest request, CancellationToken cancellationToken = default);
}
