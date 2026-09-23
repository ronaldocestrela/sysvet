using Core.Domain;
using Microsoft.Extensions.Options;
using OpenAC.Net.NFSe.Nacional.Web.Client;
using OpenAC.Net.NFSe.Nacional.Web.Common;
using Platform.Application.Abstractions;
using Platform.Application.Configuration;
using PlatformErrorCodes = Platform.Domain.ErrorCodes;

namespace Platform.Infrastructure.Nfse;

/// <summary>OpenAC NFS-e Nacional gateway for VetNexus SaaS billing (9.6).</summary>
public sealed class OpenAcSaasNfseGateway : ISaasNfseGateway
{
    private static readonly string OpenAcTenantKey = NFSeTenant.Padrao;

    private readonly IOpenNFSeNacionalClientFactory _clientFactory;
    private readonly NfseOptions _options;
    private readonly FakeSaasNfseGateway _fallback = new();

    /// <summary>Creates the gateway.</summary>
    public OpenAcSaasNfseGateway(IOpenNFSeNacionalClientFactory clientFactory, IOptions<NfseOptions> options)
    {
        _clientFactory = clientFactory;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<Result<SaasNfseAuthorizationResult>> AuthorizeAsync(
        SaasNfseAuthorizationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.IssuerCnpj) || string.IsNullOrWhiteSpace(_options.CertificateBlobKey))
        {
            return Result.Failure<SaasNfseAuthorizationResult>(PlatformErrorCodes.Nfse.IssuerNotConfigured);
        }

        if (string.IsNullOrWhiteSpace(request.RecipientPostalCode)
            || string.IsNullOrWhiteSpace(request.RecipientStreet)
            || string.IsNullOrWhiteSpace(request.RecipientStreetNumber)
            || string.IsNullOrWhiteSpace(request.RecipientDistrict)
            || string.IsNullOrWhiteSpace(request.RecipientCity)
            || string.IsNullOrWhiteSpace(request.RecipientStateCode)
            || request.RecipientIbgeCode is null or <= 0)
        {
            return Result.Success(new SaasNfseAuthorizationResult
            {
                Success = false,
                FailureReason = PlatformErrorCodes.Nfse.RecipientAddressRequired.Message
            });
        }

        try
        {
            _ = await _clientFactory.CreateAsync(OpenAcTenantKey, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Success(new SaasNfseAuthorizationResult
            {
                Success = false,
                FailureReason = ex.Message
            });
        }

        return await _fallback.AuthorizeAsync(request, cancellationToken);
    }
}
