using Core.Application.Storage;
using Microsoft.Extensions.Options;
using OpenAC.Net.NFSe.Nacional.Common;
using OpenAC.Net.NFSe.Nacional.Web.Provider;
using Platform.Application.Configuration;
using PlatformErrorCodes = Platform.Domain.ErrorCodes;

namespace Platform.Infrastructure.Nfse;

/// <summary>Loads VetNexus A1 certificate for OpenAC SaaS NFS-e (9.6).</summary>
public sealed class PlatformSaasNfseCertificateProvider : INFSeCertificateProvider
{
    private readonly IBlobStorage _blobStorage;
    private readonly NfseOptions _options;

    /// <summary>Creates the provider.</summary>
    public PlatformSaasNfseCertificateProvider(IBlobStorage blobStorage, IOptions<NfseOptions> options)
    {
        _blobStorage = blobStorage;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async ValueTask ConfigureAsync(
        string tenantId,
        NFSeCertificadoConfig certificateConfiguration,
        CancellationToken cancellationToken = default)
    {
        _ = tenantId;
        if (string.IsNullOrWhiteSpace(_options.CertificateBlobKey))
        {
            throw new InvalidOperationException(PlatformErrorCodes.Nfse.IssuerNotConfigured.Message);
        }

        var open = await _blobStorage.OpenReadAsync(_options.CertificateBlobKey, cancellationToken);
        if (open.IsFailure)
        {
            throw new InvalidOperationException(open.Error.Message);
        }

        await using var stream = open.Value;
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        certificateConfiguration.CertificadoBytes = memory.ToArray();
        certificateConfiguration.Senha = _options.CertificatePassword ?? string.Empty;
    }
}
