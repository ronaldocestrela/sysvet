using Core.Domain;
using Fiscal.Application.Abstractions.Models;

namespace Fiscal.Application.Abstractions;

/// <summary>NFS-e Padrão Nacional (ADN) transmission port.</summary>
public interface INfseGateway
{
    /// <summary>Sends DPS and returns NFS-e authorization.</summary>
    Task<Result<NfseAuthorizationResult>> AuthorizeAsync(NfseAuthorizationRequest request, CancellationToken cancellationToken = default);

    /// <summary>Cancels authorized NFS-e.</summary>
    Task<Result<NfseCancelResult>> CancelAsync(NfseCancelRequest request, CancellationToken cancellationToken = default);
}
