using Core.Domain;
using Fiscal.Application.Abstractions;
using Fiscal.Application.Abstractions.Models;
using System.Text;

namespace Fiscal.Infrastructure.Gateways;

/// <summary>Deterministic NF-e gateway for CI and development.</summary>
public sealed class FakeNfeGateway : INfeGateway
{
    public Task<Result<NfeAuthorizationResult>> AuthorizeAsync(NfeAuthorizationRequest request, CancellationToken cancellationToken = default)
    {
        var number = request.Document.Items.Count > 0 ? (int)request.Issuer.NextNfeNumber : 1;
        var key = $"3525{DateTime.UtcNow:MM}12345678901234567890123456789012345678"[..44];
        var xml = $"<nfeProc><NFe><infNFe Id=\"NFe{key}\"><ide><nNF>{number}</nNF></ide></infNFe></NFe></nfeProc>";
        return Task.FromResult(Result.Success(new NfeAuthorizationResult
        {
            Success = true,
            AccessKey = key,
            Protocol = "135250000000000",
            Xml = xml,
            Number = number,
            Series = request.Issuer.NfeSeries
        }));
    }

    public Task<Result<NfeCancelResult>> CancelAsync(NfeCancelRequest request, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(new NfeCancelResult { Success = true, Protocol = "135250000000001" }));

    public Task<Result<NfeCorrectionResult>> CorrectAsync(NfeCorrectionRequest request, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(new NfeCorrectionResult { Success = true, Protocol = "135250000000002" }));
}
