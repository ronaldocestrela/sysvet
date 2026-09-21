using Core.Domain;
using Fiscal.Domain.Repositories;
using OpenAC.Net.NFSe.Nacional.Common;
using OpenAC.Net.NFSe.Nacional.Web.Common;
using OpenAC.Net.NFSe.Nacional.Web.Provider;

namespace Fiscal.Infrastructure.OpenAc;

/// <summary>Resolves OpenAC NFS-e Nacional config per tenant from IssuerProfile.</summary>
public sealed class IssuerNfseConfigurationProvider : INFSeConfigurationProvider<ConfiguracaoNFSe>
{
    private readonly IIssuerProfileRepository _issuerRepository;

    public IssuerNfseConfigurationProvider(IIssuerProfileRepository issuerRepository)
    {
        _issuerRepository = issuerRepository;
    }

    public async ValueTask<ConfiguracaoNFSe> GetConfigurationAsync(
        string tenantId = NFSeTenant.Padrao,
        CancellationToken cancellationToken = default)
    {
        _ = tenantId;
        var issuer = await _issuerRepository.GetAsync(cancellationToken)
                     ?? throw new InvalidOperationException("Issuer profile is not configured.");

        var config = new ConfiguracaoNFSe();
        config.WebServices.CodigoMunicipio = issuer.IbgeCityCode;
        config.WebServices.ValidarSchemas = true;
        config.WebServices.Tentativas = 3;
        config.WebServices.IntervaloTentativas = 1000;
        config.WebServices.Ambiente = issuer.Environment == Fiscal.Domain.Enums.FiscalEnvironment.Production
            ? OpenAC.Net.DFe.Core.Common.DFeTipoAmbiente.Producao
            : OpenAC.Net.DFe.Core.Common.DFeTipoAmbiente.Homologacao;
        return config;
    }
}
