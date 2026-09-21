using Core.Application.Storage;
using Core.Domain;
using Fiscal.Application.Abstractions;
using Fiscal.Application.Abstractions.Models;
using Fiscal.Domain;
using Fiscal.Domain.Repositories;
using MediatR;
using System.Text;

namespace Fiscal.Application.Documents;

/// <summary>Cancels authorized fiscal document.</summary>
public sealed class CancelFiscalDocumentCommandHandler : IRequestHandler<CancelFiscalDocumentCommand, Result<bool>>
{
    private readonly IIssuerProfileRepository _issuerRepository;
    private readonly IFiscalDocumentRepository _documentRepository;
    private readonly INfeGateway _nfeGateway;
    private readonly INfseGateway _nfseGateway;
    private readonly IBlobStorage _blobStorage;
    private readonly ICertificateProtector _certificateProtector;
    private readonly ITenantContext _tenantContext;

    public CancelFiscalDocumentCommandHandler(
        IIssuerProfileRepository issuerRepository,
        IFiscalDocumentRepository documentRepository,
        INfeGateway nfeGateway,
        INfseGateway nfseGateway,
        IBlobStorage blobStorage,
        ICertificateProtector certificateProtector,
        ITenantContext tenantContext)
    {
        _issuerRepository = issuerRepository;
        _documentRepository = documentRepository;
        _nfeGateway = nfeGateway;
        _nfseGateway = nfseGateway;
        _blobStorage = blobStorage;
        _certificateProtector = certificateProtector;
        _tenantContext = tenantContext;
    }

    public async Task<Result<bool>> Handle(CancelFiscalDocumentCommand request, CancellationToken cancellationToken)
    {
        var doc = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);
        if (doc is null)
        {
            return Result.Failure<bool>(Fiscal.Domain.ErrorCodes.Document.NotFound);
        }

        var cancelResult = doc.Cancel(TimeSpan.FromHours(24));
        if (cancelResult.IsFailure)
        {
            return Result.Failure<bool>(cancelResult.Error);
        }

        var issuer = await _issuerRepository.GetAsync(cancellationToken);
        if (issuer is null)
        {
            return Result.Failure<bool>(Fiscal.Domain.ErrorCodes.Issuer.NotFound);
        }

        if (doc.DocumentType == Domain.Enums.FiscalDocumentType.Nfe)
        {
            var cert = await LoadCertificateAsync(issuer, cancellationToken);
            if (cert.IsFailure)
            {
                return Result.Failure<bool>(cert.Error);
            }

            var gatewayResult = await _nfeGateway.CancelAsync(new NfeCancelRequest
            {
                Issuer = issuer,
                Document = doc,
                CertificatePfx = cert.Value.Pfx,
                CertificatePassword = cert.Value.Password,
                Justification = request.Justification
            }, cancellationToken);

            if (gatewayResult.IsFailure || !gatewayResult.Value.Success)
            {
                return Result.Failure<bool>(new Error("Fiscal.Nfe.CancelFailed", gatewayResult.Value?.RejectionReason ?? gatewayResult.Error?.Message ?? "Cancel failed"));
            }
        }
        else
        {
            var gatewayResult = await _nfseGateway.CancelAsync(new NfseCancelRequest
            {
                TenantId = _tenantContext.TenantId,
                Issuer = issuer,
                Document = doc,
                Justification = request.Justification
            }, cancellationToken);

            if (gatewayResult.IsFailure || !gatewayResult.Value.Success)
            {
                return Result.Failure<bool>(new Error("Fiscal.Nfse.CancelFailed", gatewayResult.Value?.RejectionReason ?? gatewayResult.Error?.Message ?? "Cancel failed"));
            }
        }

        _documentRepository.Update(doc);
        return Result.Success(true);
    }

    private async Task<Result<(byte[] Pfx, string Password)>> LoadCertificateAsync(
        Domain.Entities.IssuerProfile issuer,
        CancellationToken cancellationToken)
    {
        var openResult = await _blobStorage.OpenReadAsync(issuer.CertificateBlobKey!, cancellationToken);
        if (openResult.IsFailure)
        {
            return Result.Failure<(byte[] Pfx, string Password)>(Fiscal.Domain.ErrorCodes.Issuer.CertificateMissing);
        }

        await using var stream = openResult.Value;
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, cancellationToken);
        var password = _certificateProtector.Decrypt(issuer.EncryptedCertificatePassword!);
        return Result.Success((ms.ToArray(), password));
    }
}

/// <summary>Transmits carta de correção for NF-e.</summary>
public sealed class IssueCorrectionLetterCommandHandler : IRequestHandler<IssueCorrectionLetterCommand, Result<Guid>>
{
    private readonly IIssuerProfileRepository _issuerRepository;
    private readonly IFiscalDocumentRepository _documentRepository;
    private readonly INfeGateway _nfeGateway;
    private readonly IBlobStorage _blobStorage;
    private readonly ICertificateProtector _certificateProtector;

    public IssueCorrectionLetterCommandHandler(
        IIssuerProfileRepository issuerRepository,
        IFiscalDocumentRepository documentRepository,
        INfeGateway nfeGateway,
        IBlobStorage blobStorage,
        ICertificateProtector certificateProtector)
    {
        _issuerRepository = issuerRepository;
        _documentRepository = documentRepository;
        _nfeGateway = nfeGateway;
        _blobStorage = blobStorage;
        _certificateProtector = certificateProtector;
    }

    public async Task<Result<Guid>> Handle(IssueCorrectionLetterCommand request, CancellationToken cancellationToken)
    {
        var doc = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);
        if (doc is null)
        {
            return Result.Failure<Guid>(Fiscal.Domain.ErrorCodes.Document.NotFound);
        }

        var letterResult = doc.AddCorrectionLetter(request.CorrectionText);
        if (letterResult.IsFailure)
        {
            return Result.Failure<Guid>(letterResult.Error);
        }

        var issuer = await _issuerRepository.GetAsync(cancellationToken);
        if (issuer is null)
        {
            return Result.Failure<Guid>(Fiscal.Domain.ErrorCodes.Issuer.NotFound);
        }

        var openResult = await _blobStorage.OpenReadAsync(issuer.CertificateBlobKey!, cancellationToken);
        if (openResult.IsFailure)
        {
            return Result.Failure<Guid>(Fiscal.Domain.ErrorCodes.Issuer.CertificateMissing);
        }

        await using var stream = openResult.Value;
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, cancellationToken);
        var password = _certificateProtector.Decrypt(issuer.EncryptedCertificatePassword!);

        var correctionResult = await _nfeGateway.CorrectAsync(new NfeCorrectionRequest
        {
            Issuer = issuer,
            Document = doc,
            Letter = letterResult.Value,
            CertificatePfx = ms.ToArray(),
            CertificatePassword = password
        }, cancellationToken);

        if (correctionResult.IsFailure || !correctionResult.Value.Success)
        {
            return Result.Failure<Guid>(new Error("Fiscal.Nfe.CorrectionFailed", correctionResult.Value?.RejectionReason ?? "Correction failed"));
        }

        letterResult.Value.MarkTransmitted(correctionResult.Value.Protocol ?? "FAKE");
        _documentRepository.Update(doc);
        return Result.Success(letterResult.Value.Id);
    }
}

/// <summary>Uploads and stores encrypted A1 certificate.</summary>
public sealed class UploadIssuerCertificateCommandHandler : IRequestHandler<UploadIssuerCertificateCommand, Result<bool>>
{
    private readonly IIssuerProfileRepository _issuerRepository;
    private readonly IBlobStorage _blobStorage;
    private readonly ICertificateProtector _certificateProtector;
    private readonly ITenantContext _tenantContext;

    public UploadIssuerCertificateCommandHandler(
        IIssuerProfileRepository issuerRepository,
        IBlobStorage blobStorage,
        ICertificateProtector certificateProtector,
        ITenantContext tenantContext)
    {
        _issuerRepository = issuerRepository;
        _blobStorage = blobStorage;
        _certificateProtector = certificateProtector;
        _tenantContext = tenantContext;
    }

    public async Task<Result<bool>> Handle(UploadIssuerCertificateCommand request, CancellationToken cancellationToken)
    {
        var issuer = await _issuerRepository.GetAsync(cancellationToken);
        if (issuer is null)
        {
            return Result.Failure<bool>(Fiscal.Domain.ErrorCodes.Issuer.NotFound);
        }

        var key = $"{_tenantContext.SchemaName}/fiscal/certificates/{issuer.Id}.pfx.enc";
        await _blobStorage.PutAsync(key, new MemoryStream(request.PfxBytes), "application/x-pkcs12", cancellationToken);
        issuer.SetCertificate(key, _certificateProtector.Encrypt(request.Password));
        _issuerRepository.Update(issuer);
        return Result.Success(true);
    }
}
