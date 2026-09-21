using Core.Domain;
using Fiscal.Application.Abstractions;
using Fiscal.Application.Abstractions.Models;
using Fiscal.Domain;
using OpenAC.Net.NFSe.Nacional.Web.Client;

namespace Fiscal.Infrastructure.Gateways;

/// <summary>
/// NFS-e Padrão Nacional via OpenAC.Net.NFSe.Nacional.Web.
/// Validates tenant OpenAC client bootstrap; emission uses deterministic fake payload until homologation DPS mapping.
/// </summary>
public sealed class OpenAcNacionalWebNfseGateway : INfseGateway
{
    private readonly IOpenNFSeNacionalClientFactory _clientFactory;
    private readonly FakeNfseGateway _fallback = new();

    public OpenAcNacionalWebNfseGateway(IOpenNFSeNacionalClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<Result<NfseAuthorizationResult>> AuthorizeAsync(
        NfseAuthorizationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _ = await _clientFactory.CreateAsync(request.TenantId.ToString(), cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<NfseAuthorizationResult>(Fiscal.Domain.ErrorCodes.Nfse.NationalEnvironmentUnavailable with { Message = ex.Message });
        }

        return await _fallback.AuthorizeAsync(request, cancellationToken);
    }

    public async Task<Result<NfseCancelResult>> CancelAsync(NfseCancelRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _ = await _clientFactory.CreateAsync(request.TenantId.ToString(), cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<NfseCancelResult>(new Error("Fiscal.Nfse.CancelFailed", ex.Message));
        }

        return await _fallback.CancelAsync(request, cancellationToken);
    }
}
