using Core.Application.IntegrationEvents;
using Core.Application.Storage;
using Core.Domain;
using Fiscal.Application.Abstractions;
using Fiscal.Application.Abstractions.Models;
using Fiscal.Domain;
using Fiscal.Domain.Entities;
using Fiscal.Domain.Enums;
using Fiscal.Domain.Repositories;
using Fiscal.Domain.Services;
using MediatR;
using System.Text;

namespace Fiscal.Application.Documents;

/// <summary>Issues NF-e and/or NFS-e Nacional from a paid order.</summary>
public sealed class IssueFromOrderCommandHandler : IRequestHandler<IssueFromOrderCommand, Result<IReadOnlyList<Guid>>>
{
    private readonly IMediator _mediator;
    private readonly IIssuerProfileRepository _issuerRepository;
    private readonly IFiscalDocumentRepository _documentRepository;
    private readonly INfeGateway _nfeGateway;
    private readonly INfseGateway _nfseGateway;
    private readonly IDanfeRenderer _danfeRenderer;
    private readonly IBlobStorage _blobStorage;
    private readonly ICertificateProtector _certificateProtector;
    private readonly ITenantContext _tenantContext;
    private readonly IFiscalUnitOfWork _fiscalUnitOfWork;

    public IssueFromOrderCommandHandler(
        IMediator mediator,
        IIssuerProfileRepository issuerRepository,
        IFiscalDocumentRepository documentRepository,
        INfeGateway nfeGateway,
        INfseGateway nfseGateway,
        IDanfeRenderer danfeRenderer,
        IBlobStorage blobStorage,
        ICertificateProtector certificateProtector,
        ITenantContext tenantContext,
        IFiscalUnitOfWork fiscalUnitOfWork)
    {
        _mediator = mediator;
        _issuerRepository = issuerRepository;
        _documentRepository = documentRepository;
        _nfeGateway = nfeGateway;
        _nfseGateway = nfseGateway;
        _danfeRenderer = danfeRenderer;
        _blobStorage = blobStorage;
        _certificateProtector = certificateProtector;
        _tenantContext = tenantContext;
        _fiscalUnitOfWork = fiscalUnitOfWork;
    }

    public async Task<Result<IReadOnlyList<Guid>>> Handle(IssueFromOrderCommand request, CancellationToken cancellationToken)
    {
        var issuer = await _issuerRepository.GetAsync(cancellationToken);
        if (issuer is null)
        {
            return Result.Failure<IReadOnlyList<Guid>>(Fiscal.Domain.ErrorCodes.Issuer.NotFound);
        }

        if (!issuer.HasCertificate)
        {
            return Result.Failure<IReadOnlyList<Guid>>(Fiscal.Domain.ErrorCodes.Issuer.CertificateMissing);
        }

        var snapshotResult = await _mediator.Send(new GetPaidOrderFiscalSnapshotRequest(request.OrderId), cancellationToken);
        if (snapshotResult.IsFailure)
        {
            return Result.Failure<IReadOnlyList<Guid>>(snapshotResult.Error);
        }

        var snapshot = snapshotResult.Value;
        if (!snapshot.IsPaid)
        {
            return Result.Failure<IReadOnlyList<Guid>>(Fiscal.Domain.ErrorCodes.Document.OrderNotPaid);
        }

        TutorFiscalDest? tutor = null;
        if (snapshot.TutorId is Guid tutorId)
        {
            var tutorResult = await _mediator.Send(new GetTutorFiscalDestRequest(tutorId), cancellationToken);
            if (tutorResult.IsSuccess)
            {
                tutor = tutorResult.Value;
            }
        }

        var existingNfce = await _documentRepository.GetByOrderAndTypeAsync(request.OrderId, FiscalDocumentType.Nfce, cancellationToken);
        var skipNfeBecauseNfce = existingNfce is not null
            && existingNfce.Status is FiscalDocumentStatus.Authorized
                or FiscalDocumentStatus.ContingencyIssued
                or FiscalDocumentStatus.Transmitting;

        var nfeLines = skipNfeBecauseNfce
            ? []
            : snapshot.Lines.Where(l => OrderFiscalLineKind.IsNfeLine(l.Kind)).ToList();
        var nfseLines = snapshot.Lines.Where(l => OrderFiscalLineKind.IsNfseLine(l.Kind)).ToList();
        if (nfeLines.Count == 0 && nfseLines.Count == 0)
        {
            if (existingNfce is not null)
            {
                return Result.Success<IReadOnlyList<Guid>>([existingNfce.Id]);
            }

            return Result.Failure<IReadOnlyList<Guid>>(Fiscal.Domain.ErrorCodes.Document.NoLines);
        }

        var productIds = nfeLines.Where(l => l.ProductId.HasValue).Select(l => l.ProductId!.Value).Distinct().ToList();
        var basicsResult = await _mediator.Send(new GetProductsFiscalBasicsRequest(productIds), cancellationToken);
        var basics = basicsResult.IsSuccess
            ? basicsResult.Value.ToDictionary(p => p.ProductId)
            : new Dictionary<Guid, ProductFiscalBasic>();

        var cert = await LoadCertificateAsync(issuer, cancellationToken);
        if (cert.IsFailure)
        {
            return Result.Failure<IReadOnlyList<Guid>>(cert.Error);
        }

        var issuedIds = new List<Guid>();
        var needNfe = nfeLines.Count > 0;
        var needNfse = nfseLines.Count > 0;

        if (needNfe)
        {
            var existing = await _documentRepository.GetAuthorizedByOrderAsync(request.OrderId, FiscalDocumentType.Nfe, cancellationToken);
            if (existing is not null)
            {
                issuedIds.Add(existing.Id);
            }
            else
            {
                var nfeResult = await IssueNfeAsync(
                    issuer,
                    request.OrderId,
                    nfeLines,
                    basics,
                    tutor,
                    cert.Value.Pfx,
                    cert.Value.Password,
                    cancellationToken);
                if (nfeResult.IsFailure)
                {
                    return Result.Failure<IReadOnlyList<Guid>>(nfeResult.Error);
                }

                issuedIds.Add(nfeResult.Value);
            }
        }

        if (needNfse)
        {
            var existing = await _documentRepository.GetAuthorizedByOrderAsync(request.OrderId, FiscalDocumentType.Nfse, cancellationToken);
            if (existing is not null)
            {
                issuedIds.Add(existing.Id);
            }
            else
            {
                var nfseResult = await IssueNfseAsync(issuer, request.OrderId, nfseLines, tutor, cancellationToken);
                if (nfseResult.IsFailure)
                {
                    return Result.Failure<IReadOnlyList<Guid>>(nfseResult.Error);
                }

                issuedIds.Add(nfseResult.Value);
            }
        }

        var nfeAuthorized = !needNfe || await _documentRepository.GetAuthorizedByOrderAsync(request.OrderId, FiscalDocumentType.Nfe, cancellationToken) is not null;
        var nfseAuthorized = !needNfse || await _documentRepository.GetAuthorizedByOrderAsync(request.OrderId, FiscalDocumentType.Nfse, cancellationToken) is not null;
        var partial = needNfe && needNfse && !(nfeAuthorized && nfseAuthorized);
        await _mediator.Send(new MarkOrderFiscalLinkedRequest(request.OrderId, partial), cancellationToken);

        return Result.Success<IReadOnlyList<Guid>>(issuedIds);
    }

    private async Task<Result<Guid>> IssueNfeAsync(
        IssuerProfile issuer,
        Guid orderId,
        IReadOnlyList<PaidOrderFiscalLine> lines,
        IReadOnlyDictionary<Guid, ProductFiscalBasic> basics,
        TutorFiscalDest? tutor,
        byte[] pfx,
        string password,
        CancellationToken cancellationToken)
    {
        if (tutor is null || string.IsNullOrWhiteSpace(tutor.Street) || string.IsNullOrWhiteSpace(tutor.State))
        {
            return Result.Failure<Guid>(Fiscal.Domain.ErrorCodes.Document.DestAddressRequired);
        }

        var recipientUf = tutor.State;
        var docResult = FiscalDocument.CreateDraft(
            FiscalDocumentType.Nfe,
            orderId,
            tutor.Name,
            tutor.Cpf,
            recipientUf);
        if (docResult.IsFailure)
        {
            return Result.Failure<Guid>(docResult.Error);
        }

        var doc = docResult.Value;
        foreach (var line in lines)
        {
            var ncm = line.ProductId is Guid pid && basics.TryGetValue(pid, out var basic)
                ? basic.Ncm
                : "00000000";
            var origin = line.ProductId is Guid pid2 && basics.TryGetValue(pid2, out var basic2)
                ? basic2.MerchandiseOrigin
                : 0;
            var cfop = FiscalTaxResolver.ResolveProductCfop(issuer.State, recipientUf);
            var add = doc.AddProductItem(line.ProductId, line.Description, line.Quantity, line.UnitPrice, ncm, cfop, FiscalTaxResolver.DefaultCsosn, origin);
            if (add.IsFailure)
            {
                return Result.Failure<Guid>(add.Error);
            }
        }

        var begin = doc.BeginTransmission();
        if (begin.IsFailure)
        {
            return Result.Failure<Guid>(begin.Error);
        }
        _documentRepository.Add(doc);
        await _fiscalUnitOfWork.SaveChangesAsync(cancellationToken);

        var nfeNumber = (int)issuer.ConsumeNextNfeNumber();
        var authResult = await _nfeGateway.AuthorizeAsync(new NfeAuthorizationRequest
        {
            Issuer = issuer,
            Document = doc,
            CertificatePfx = pfx,
            CertificatePassword = password,
            RecipientName = tutor.Name,
            RecipientCpf = tutor.Cpf,
            RecipientStreet = tutor.Street,
            RecipientNumber = tutor.Number,
            RecipientDistrict = tutor.District,
            RecipientCity = tutor.City,
            RecipientState = tutor.State,
            RecipientPostalCode = tutor.PostalCode,
            RecipientIbge = tutor.IbgeCityCode
        }, cancellationToken);

        if (authResult.IsFailure)
        {
            doc.MarkRejected(authResult.Error.Message);
            _documentRepository.Update(doc);
            return Result.Failure<Guid>(authResult.Error);
        }

        var auth = authResult.Value;
        if (!auth.Success)
        {
            doc.MarkRejected(auth.RejectionReason ?? "SEFAZ rejected NF-e.");
            _documentRepository.Update(doc);
            return Result.Failure<Guid>(new Error("Fiscal.Nfe.Rejected", auth.RejectionReason ?? "SEFAZ rejected NF-e."));
        }

        var xmlKey = BuildBlobKey(doc.Id, "xml");
        await _blobStorage.PutAsync(xmlKey, new MemoryStream(Encoding.UTF8.GetBytes(auth.Xml ?? "<nfe/>")), "application/xml", cancellationToken);

        string? danfeKey = null;
        if (!string.IsNullOrWhiteSpace(auth.Xml))
        {
            var pdfResult = await _danfeRenderer.RenderAsync(auth.Xml, cancellationToken);
            if (pdfResult.IsSuccess)
            {
                danfeKey = BuildBlobKey(doc.Id, "danfe.pdf");
                await _blobStorage.PutAsync(danfeKey, new MemoryStream(pdfResult.Value), "application/pdf", cancellationToken);
            }
        }

        var authorized = doc.MarkAuthorized(auth.AccessKey, auth.Protocol ?? "FAKE", xmlKey, danfeKey, nfeNumber, issuer.NfeSeries, null);
        if (authorized.IsFailure)
        {
            return Result.Failure<Guid>(authorized.Error);
        }

        _documentRepository.Update(doc);
        _issuerRepository.Update(issuer);
        return Result.Success(doc.Id);
    }

    private async Task<Result<Guid>> IssueNfseAsync(
        IssuerProfile issuer,
        Guid orderId,
        IReadOnlyList<PaidOrderFiscalLine> lines,
        TutorFiscalDest? tutor,
        CancellationToken cancellationToken)
    {
        var docResult = FiscalDocument.CreateDraft(
            FiscalDocumentType.Nfse,
            orderId,
            tutor?.Name ?? "Consumidor",
            tutor?.Cpf,
            tutor?.State);
        if (docResult.IsFailure)
        {
            return Result.Failure<Guid>(docResult.Error);
        }

        var doc = docResult.Value;
        foreach (var line in lines)
        {
            doc.AddServiceItem(line.Description, line.Quantity, line.UnitPrice);
        }

        doc.BeginTransmission();
        _documentRepository.Add(doc);
        await _fiscalUnitOfWork.SaveChangesAsync(cancellationToken);

        issuer.ConsumeNextDpsNumber();
        var authResult = await _nfseGateway.AuthorizeAsync(new NfseAuthorizationRequest
        {
            TenantId = _tenantContext.TenantId,
            Issuer = issuer,
            Document = doc
        }, cancellationToken);

        if (authResult.IsFailure)
        {
            doc.MarkRejected(authResult.Error.Message);
            _documentRepository.Update(doc);
            return Result.Failure<Guid>(authResult.Error);
        }

        var auth = authResult.Value;
        if (!auth.Success)
        {
            doc.MarkRejected(auth.RejectionReason ?? "ADN rejected NFS-e.");
            _documentRepository.Update(doc);
            return Result.Failure<Guid>(new Error("Fiscal.Nfse.Rejected", auth.RejectionReason ?? "ADN rejected NFS-e."));
        }

        var xmlKey = BuildBlobKey(doc.Id, "nfse.xml");
        await _blobStorage.PutAsync(xmlKey, new MemoryStream(Encoding.UTF8.GetBytes(auth.Xml ?? "<nfse/>")), "application/xml", cancellationToken);
        doc.MarkAuthorized(auth.AccessKey, auth.NfseNumber ?? "0", xmlKey, null, null, null, auth.NfseNumber);
        _documentRepository.Update(doc);
        _issuerRepository.Update(issuer);
        return Result.Success(doc.Id);
    }

    private async Task<Result<(byte[] Pfx, string Password)>> LoadCertificateAsync(
        IssuerProfile issuer,
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

    private string BuildBlobKey(Guid documentId, string suffix) =>
        $"{_tenantContext.SchemaName}/fiscal/{DateTime.UtcNow:yyyy}/{DateTime.UtcNow:MM}/{documentId}.{suffix}";
}
