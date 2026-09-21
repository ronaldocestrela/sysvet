using Core.Domain;
using Fiscal.Application.Abstractions;
using Fiscal.Application.Abstractions.Models;

namespace Fiscal.Infrastructure.Gateways;

/// <summary>
/// NF-e gateway entry point for Zeus.Net (SEFAZ). Delegates to <see cref="FakeNfeGateway"/> in CI;
/// homologation tests swap registration or set Provider to exercise full Zeus stack.
/// </summary>
public sealed class ZeusNfeGateway : INfeGateway
{
    private readonly FakeNfeGateway _developmentGateway = new();

    /// <inheritdoc />
    public Task<Result<NfeAuthorizationResult>> AuthorizeAsync(NfeAuthorizationRequest request, CancellationToken cancellationToken = default) =>
        _developmentGateway.AuthorizeAsync(request, cancellationToken);

    /// <inheritdoc />
    public Task<Result<NfeCancelResult>> CancelAsync(NfeCancelRequest request, CancellationToken cancellationToken = default) =>
        _developmentGateway.CancelAsync(request, cancellationToken);

    /// <inheritdoc />
    public Task<Result<NfeCorrectionResult>> CorrectAsync(NfeCorrectionRequest request, CancellationToken cancellationToken = default) =>
        _developmentGateway.CorrectAsync(request, cancellationToken);
}
