using Core.Domain;
using Fiscal.Application.Abstractions.Models;

namespace Fiscal.Application.Abstractions;

/// <summary>SEFAZ NF-e transmission port.</summary>
public interface INfeGateway
{
    /// <summary>Authorizes NF-e model 55.</summary>
    Task<Result<NfeAuthorizationResult>> AuthorizeAsync(NfeAuthorizationRequest request, CancellationToken cancellationToken = default);

    /// <summary>Cancels authorized NF-e.</summary>
    Task<Result<NfeCancelResult>> CancelAsync(NfeCancelRequest request, CancellationToken cancellationToken = default);

    /// <summary>Transmits carta de correção.</summary>
    Task<Result<NfeCorrectionResult>> CorrectAsync(NfeCorrectionRequest request, CancellationToken cancellationToken = default);
}
