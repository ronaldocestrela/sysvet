using Core.Application.IntegrationEvents;
using Core.Application.Storage;
using Core.Domain;
using Fiscal.Application.Abstractions;
using Fiscal.Application.Abstractions.Models;
using Fiscal.Domain.Entities;
using FiscalErrorCodes = Fiscal.Domain.ErrorCodes;
using Fiscal.Domain.Enums;
using Fiscal.Domain.Repositories;
using MediatR;
using System.Text;

namespace Fiscal.Application.Documents;

/// <summary>Ingests PDV NFC-e and transmits to SEFAZ.</summary>
public sealed class TransmitNfceCommandHandler : IRequestHandler<TransmitNfceCommand, Result<bool>>
{
    private readonly IIssuerProfileRepository _issuerRepository;
    private readonly IFiscalDocumentRepository _documentRepository;
    private readonly INfceGateway _nfceGateway;
    private readonly INfceDanfeRenderer _danfeRenderer;
    private readonly IBlobStorage _blobStorage;
    private readonly ICertificateProtector _certificateProtector;
    private readonly ITenantContext _tenantContext;
    private readonly IFiscalUnitOfWork _fiscalUnitOfWork;
    private readonly IMediator _mediator;

    public TransmitNfceCommandHandler(
        IIssuerProfileRepository issuerRepository,
        IFiscalDocumentRepository documentRepository,
        INfceGateway nfceGateway,
        INfceDanfeRenderer danfeRenderer,
        IBlobStorage blobStorage,
        ICertificateProtector certificateProtector,
        ITenantContext tenantContext,
        IFiscalUnitOfWork fiscalUnitOfWork,
        IMediator mediator)
    {
        _issuerRepository = issuerRepository;
        _documentRepository = documentRepository;
        _nfceGateway = nfceGateway;
        _danfeRenderer = danfeRenderer;
        _blobStorage = blobStorage;
        _certificateProtector = certificateProtector;
        _tenantContext = tenantContext;
        _fiscalUnitOfWork = fiscalUnitOfWork;
        _mediator = mediator;
    }

    public async Task<Result<bool>> Handle(TransmitNfceCommand request, CancellationToken cancellationToken)
    {
        var existing = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);
        if (existing is not null && existing.Status == FiscalDocumentStatus.Authorized)
        {
            return Result.Success(true);
        }

        var issuer = await _issuerRepository.GetAsync(cancellationToken);
        if (issuer is null || !issuer.HasCertificate)
        {
            return Result.Failure<bool>(FiscalErrorCodes.Issuer.CertificateMissing);
        }

        var cert = await LoadCertificateAsync(issuer, cancellationToken);
        if (cert.IsFailure)
        {
            return Result.Failure<bool>(cert.Error);
        }

        FiscalDocument doc;
        if (existing is not null)
        {
            doc = existing;
        }
        else
        {
            var draftResult = FiscalDocument.CreateDraft(
                FiscalDocumentType.Nfce,
                request.OrderId,
                request.RecipientName,
                request.RecipientCpf,
                request.RecipientUf,
                request.DocumentId);
            if (draftResult.IsFailure)
            {
                return Result.Failure<bool>(draftResult.Error);
            }

            doc = draftResult.Value;
            foreach (var line in request.Lines)
            {
                var add = doc.AddProductItem(
                    line.ProductId,
                    line.Description,
                    line.Quantity,
                    line.UnitPrice,
                    line.Ncm,
                    line.Cfop,
                    line.Csosn,
                    line.MerchandiseOrigin);
                if (add.IsFailure)
                {
                    return Result.Failure<bool>(add.Error);
                }
            }

            var xmlKey = BuildBlobKey(doc.Id, "contingency.xml");
            await _blobStorage.PutAsync(
                xmlKey,
                new MemoryStream(Encoding.UTF8.GetBytes(request.SignedXml)),
                "application/xml",
                cancellationToken);

            var contingency = doc.MarkContingencyIssued(
                request.AccessKey,
                xmlKey,
                request.QrCodeUrl,
                request.Number,
                request.Series,
                request.EmissionType);
            if (contingency.IsFailure)
            {
                return Result.Failure<bool>(contingency.Error);
            }

            _documentRepository.Add(doc);
            await _fiscalUnitOfWork.SaveChangesAsync(cancellationToken);
        }

        if (doc.Status == FiscalDocumentStatus.Transmitting)
        {
            return Result.Success(true);
        }

        doc.BeginTransmission();
        _documentRepository.Update(doc);
        await _fiscalUnitOfWork.SaveChangesAsync(cancellationToken);

        var transmitResult = await _nfceGateway.TransmitAsync(new NfceTransmitRequest
        {
            Issuer = issuer,
            Document = doc,
            SignedXml = request.SignedXml,
            CertificatePfx = cert.Value.Pfx,
            CertificatePassword = cert.Value.Password
        }, cancellationToken);

        if (transmitResult.IsFailure)
        {
            doc.MarkRejected(transmitResult.Error.Message);
            _documentRepository.Update(doc);
            return Result.Failure<bool>(transmitResult.Error);
        }

        var outcome = transmitResult.Value;
        if (!outcome.Success)
        {
            doc.MarkRejected(outcome.RejectionReason ?? "SEFAZ rejected NFC-e.");
            _documentRepository.Update(doc);
            return Result.Failure<bool>(new Error("Fiscal.Nfce.Rejected", outcome.RejectionReason ?? "SEFAZ rejected NFC-e."));
        }

        var authorizedXml = outcome.AuthorizedXml ?? request.SignedXml;
        var authXmlKey = BuildBlobKey(doc.Id, "nfce.xml");
        await _blobStorage.PutAsync(
            authXmlKey,
            new MemoryStream(Encoding.UTF8.GetBytes(authorizedXml)),
            "application/xml",
            cancellationToken);

        string? danfeKey = null;
        var pdfResult = await _danfeRenderer.RenderAsync(authorizedXml, doc.QrCodeUrl, cancellationToken);
        if (pdfResult.IsSuccess)
        {
            danfeKey = BuildBlobKey(doc.Id, "nfce.pdf");
            await _blobStorage.PutAsync(danfeKey, new MemoryStream(pdfResult.Value), "application/pdf", cancellationToken);
        }

        doc.MarkAuthorized(doc.AccessKey, outcome.Protocol ?? "FAKE", authXmlKey, danfeKey, doc.NfeNumber, doc.NfeSeries, null);
        _documentRepository.Update(doc);
        await _fiscalUnitOfWork.SaveChangesAsync(cancellationToken);
        await _mediator.Send(new MarkOrderFiscalLinkedRequest(request.OrderId, partial: false), cancellationToken);
        return Result.Success(true);
    }

    private async Task<Result<(byte[] Pfx, string Password)>> LoadCertificateAsync(
        IssuerProfile issuer,
        CancellationToken cancellationToken)
    {
        var openResult = await _blobStorage.OpenReadAsync(issuer.CertificateBlobKey!, cancellationToken);
        if (openResult.IsFailure)
        {
            return Result.Failure<(byte[] Pfx, string Password)>(FiscalErrorCodes.Issuer.CertificateMissing);
        }

        await using var stream = openResult.Value;
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, cancellationToken);
        var password = _certificateProtector.Decrypt(issuer.EncryptedCertificatePassword!);
        return Result.Success((ms.ToArray(), password));
    }

    private string BuildBlobKey(Guid documentId, string suffix) =>
        $"{_tenantContext.SchemaName}/fiscal/{DateTime.UtcNow:yyyy}/{DateTime.UtcNow:MM}/{documentId}.{suffix}";
}

/// <summary>Queries SEFAZ for NFC-e authorization status.</summary>
public sealed class ReconcileNfceStatusCommandHandler : IRequestHandler<ReconcileNfceStatusCommand, Result<bool>>
{
    private readonly IIssuerProfileRepository _issuerRepository;
    private readonly IFiscalDocumentRepository _documentRepository;
    private readonly INfceGateway _nfceGateway;
    private readonly ICertificateProtector _certificateProtector;
    private readonly IBlobStorage _blobStorage;
    private readonly IFiscalUnitOfWork _fiscalUnitOfWork;

    public ReconcileNfceStatusCommandHandler(
        IIssuerProfileRepository issuerRepository,
        IFiscalDocumentRepository documentRepository,
        INfceGateway nfceGateway,
        ICertificateProtector certificateProtector,
        IBlobStorage blobStorage,
        IFiscalUnitOfWork fiscalUnitOfWork)
    {
        _issuerRepository = issuerRepository;
        _documentRepository = documentRepository;
        _nfceGateway = nfceGateway;
        _certificateProtector = certificateProtector;
        _blobStorage = blobStorage;
        _fiscalUnitOfWork = fiscalUnitOfWork;
    }

    public async Task<Result<bool>> Handle(ReconcileNfceStatusCommand request, CancellationToken cancellationToken)
    {
        var doc = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);
        if (doc is null)
        {
            return Result.Failure<bool>(FiscalErrorCodes.Document.NotFound);
        }

        if (doc.DocumentType != FiscalDocumentType.Nfce || string.IsNullOrWhiteSpace(doc.AccessKey))
        {
            return Result.Failure<bool>(FiscalErrorCodes.Document.InvalidTransition);
        }

        if (doc.Status == FiscalDocumentStatus.Authorized)
        {
            return Result.Success(true);
        }

        var issuer = await _issuerRepository.GetAsync(cancellationToken);
        if (issuer is null || !issuer.HasCertificate)
        {
            return Result.Failure<bool>(FiscalErrorCodes.Issuer.CertificateMissing);
        }

        var openResult = await _blobStorage.OpenReadAsync(issuer.CertificateBlobKey!, cancellationToken);
        if (openResult.IsFailure)
        {
            return Result.Failure<bool>(FiscalErrorCodes.Issuer.CertificateMissing);
        }

        await using var stream = openResult.Value;
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, cancellationToken);
        var password = _certificateProtector.Decrypt(issuer.EncryptedCertificatePassword!);

        var status = await _nfceGateway.QueryStatusAsync(new NfceStatusRequest
        {
            Issuer = issuer,
            AccessKey = doc.AccessKey,
            CertificatePfx = ms.ToArray(),
            CertificatePassword = password
        }, cancellationToken);

        if (status.IsFailure)
        {
            return Result.Failure<bool>(status.Error);
        }

        if (status.Value.Authorized && doc.Status != FiscalDocumentStatus.Authorized)
        {
            doc.BeginTransmission();
            doc.MarkAuthorized(doc.AccessKey, status.Value.Protocol ?? "RECON", doc.XmlBlobKey ?? string.Empty, doc.DanfeBlobKey, doc.NfeNumber, doc.NfeSeries, null);
            _documentRepository.Update(doc);
        }
        else if (!status.Value.Authorized && !string.IsNullOrWhiteSpace(status.Value.RejectionReason))
        {
            doc.MarkRejected(status.Value.RejectionReason);
            _documentRepository.Update(doc);
        }

        return Result.Success(true);
    }
}

/// <summary>Returns encrypted issuer bundle for PDV cache.</summary>
public sealed class GetFiscalPosBundleQueryHandler : IRequestHandler<GetFiscalPosBundleQuery, Result<FiscalPosBundleDto?>>
{
    private readonly IIssuerProfileRepository _issuerRepository;
    private readonly IBlobStorage _blobStorage;

    public GetFiscalPosBundleQueryHandler(IIssuerProfileRepository issuerRepository, IBlobStorage blobStorage)
    {
        _issuerRepository = issuerRepository;
        _blobStorage = blobStorage;
    }

    public async Task<Result<FiscalPosBundleDto?>> Handle(GetFiscalPosBundleQuery request, CancellationToken cancellationToken)
    {
        var issuer = await _issuerRepository.GetAsync(cancellationToken);
        if (issuer is null)
        {
            return Result.Success<FiscalPosBundleDto?>(null);
        }

        string? pfxBase64 = null;
        if (issuer.HasCertificate)
        {
            var open = await _blobStorage.OpenReadAsync(issuer.CertificateBlobKey!, cancellationToken);
            if (open.IsSuccess)
            {
                await using var stream = open.Value;
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms, cancellationToken);
                pfxBase64 = Convert.ToBase64String(ms.ToArray());
            }
        }

        return Result.Success<FiscalPosBundleDto?>(new FiscalPosBundleDto(
            issuer.Id,
            issuer.LegalName,
            issuer.TradeName,
            issuer.Cnpj.Value,
            issuer.State,
            issuer.IbgeCityCode,
            issuer.NfceSeries,
            issuer.NfeSeries,
            issuer.Environment,
            issuer.HasCertificate,
            pfxBase64,
            issuer.EncryptedCertificatePassword));
    }
}
