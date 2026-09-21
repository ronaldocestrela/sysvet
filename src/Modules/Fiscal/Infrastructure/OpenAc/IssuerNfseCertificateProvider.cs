using Core.Application.Storage;
using Fiscal.Application.Abstractions;
using Fiscal.Domain;
using Fiscal.Domain.Repositories;
using OpenAC.Net.NFSe.Nacional.Common;
using OpenAC.Net.NFSe.Nacional.Web.Provider;

namespace Fiscal.Infrastructure.OpenAc;

/// <summary>Loads tenant A1 certificate into OpenAC NFS-e client config.</summary>
public sealed class IssuerNfseCertificateProvider : INFSeCertificateProvider
{
    private readonly IIssuerProfileRepository _issuerRepository;
    private readonly IBlobStorage _blobStorage;
    private readonly ICertificateProtector _certificateProtector;

    public IssuerNfseCertificateProvider(
        IIssuerProfileRepository issuerRepository,
        IBlobStorage blobStorage,
        ICertificateProtector certificateProtector)
    {
        _issuerRepository = issuerRepository;
        _blobStorage = blobStorage;
        _certificateProtector = certificateProtector;
    }

    public async ValueTask ConfigureAsync(
        string tenantId,
        NFSeCertificadoConfig certificateConfiguration,
        CancellationToken cancellationToken = default)
    {
        _ = tenantId;
        var issuer = await _issuerRepository.GetAsync(cancellationToken);
        if (issuer is null || !issuer.HasCertificate)
        {
            throw new InvalidOperationException(ErrorCodes.Issuer.CertificateMissing.Message);
        }

        var open = await _blobStorage.OpenReadAsync(issuer.CertificateBlobKey!, cancellationToken);
        if (open.IsFailure)
        {
            throw new InvalidOperationException(ErrorCodes.Issuer.CertificateMissing.Message);
        }

        await using var stream = open.Value;
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, cancellationToken);
        certificateConfiguration.CertificadoBytes = ms.ToArray();
        certificateConfiguration.Senha = _certificateProtector.Decrypt(issuer.EncryptedCertificatePassword!);
    }
}
