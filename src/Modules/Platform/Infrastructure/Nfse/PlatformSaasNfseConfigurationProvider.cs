using Microsoft.Extensions.Options;
using OpenAC.Net.DFe.Core.Common;
using OpenAC.Net.NFSe.Nacional.Common;
using OpenAC.Net.NFSe.Nacional.Web.Common;
using OpenAC.Net.NFSe.Nacional.Web.Provider;
using Platform.Application.Configuration;

namespace Platform.Infrastructure.Nfse;

/// <summary>Maps VetNexus issuer settings into OpenAC NFS-e config (9.6).</summary>
public sealed class PlatformSaasNfseConfigurationProvider : INFSeConfigurationProvider<ConfiguracaoNFSe>
{
    private readonly NfseOptions _options;

    /// <summary>Creates the provider.</summary>
    public PlatformSaasNfseConfigurationProvider(IOptions<NfseOptions> options) => _options = options.Value;

    /// <inheritdoc />
    public ValueTask<ConfiguracaoNFSe> GetConfigurationAsync(
        string tenantId = NFSeTenant.Padrao,
        CancellationToken cancellationToken = default)
    {
        _ = tenantId;
        var config = new ConfiguracaoNFSe();
        config.WebServices.CodigoMunicipio = _options.IssuerIbgeCityCode;
        config.WebServices.ValidarSchemas = true;
        config.WebServices.Tentativas = 3;
        config.WebServices.IntervaloTentativas = 1000;
        config.WebServices.Ambiente = string.Equals(_options.Environment, "Production", StringComparison.OrdinalIgnoreCase)
            ? DFeTipoAmbiente.Producao
            : DFeTipoAmbiente.Homologacao;
        return ValueTask.FromResult(config);
    }
}
