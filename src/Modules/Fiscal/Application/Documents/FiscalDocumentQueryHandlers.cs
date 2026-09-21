using Core.Application.Storage;
using Core.Domain;
using Fiscal.Domain.Repositories;
using MediatR;

namespace Fiscal.Application.Documents;

/// <summary>Lists fiscal documents.</summary>
public sealed class ListFiscalDocumentsQueryHandler
    : IRequestHandler<ListFiscalDocumentsQuery, Result<IReadOnlyList<FiscalDocumentDto>>>
{
    private readonly IFiscalDocumentRepository _repository;

    public ListFiscalDocumentsQueryHandler(IFiscalDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<FiscalDocumentDto>>> Handle(
        ListFiscalDocumentsQuery request,
        CancellationToken cancellationToken)
    {
        var docs = await _repository.ListAsync(request.OrderId, request.Status, cancellationToken);
        var dtos = docs.Select(d => new FiscalDocumentDto(
            d.Id,
            d.DocumentType,
            d.Status,
            d.SourceOrderId,
            d.AccessKey,
            d.TotalAmount,
            d.UpdatedAt,
            d.AuthorizedAt)).ToList();
        return Result.Success<IReadOnlyList<FiscalDocumentDto>>(dtos);
    }
}

/// <summary>Gets fiscal document detail.</summary>
public sealed class GetFiscalDocumentByIdQueryHandler
    : IRequestHandler<GetFiscalDocumentByIdQuery, Result<FiscalDocumentDetailDto?>>
{
    private readonly IFiscalDocumentRepository _repository;

    public GetFiscalDocumentByIdQueryHandler(IFiscalDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<FiscalDocumentDetailDto?>> Handle(
        GetFiscalDocumentByIdQuery request,
        CancellationToken cancellationToken)
    {
        var doc = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (doc is null)
        {
            return Result.Success<FiscalDocumentDetailDto?>(null);
        }

        return Result.Success<FiscalDocumentDetailDto?>(new FiscalDocumentDetailDto(
            doc.Id,
            doc.DocumentType,
            doc.Status,
            doc.SourceOrderId,
            doc.AccessKey,
            doc.Protocol,
            doc.RejectionReason,
            doc.RecipientName,
            doc.TotalAmount,
            doc.Items.Select(i => new FiscalDocumentLineDto(i.Description, i.Quantity, i.UnitPrice, i.Ncm, i.Cfop)).ToList(),
            doc.Corrections.Select(c => new FiscalCorrectionDto(c.Sequence, c.CorrectionText, c.Protocol)).ToList()));
    }
}

/// <summary>Downloads stored XML.</summary>
public sealed class DownloadFiscalXmlQueryHandler
    : IRequestHandler<DownloadFiscalXmlQuery, Result<FiscalFileDownloadDto?>>
{
    private readonly IFiscalDocumentRepository _repository;
    private readonly IBlobStorage _blobStorage;

    public DownloadFiscalXmlQueryHandler(IFiscalDocumentRepository repository, IBlobStorage blobStorage)
    {
        _repository = repository;
        _blobStorage = blobStorage;
    }

    public async Task<Result<FiscalFileDownloadDto?>> Handle(DownloadFiscalXmlQuery request, CancellationToken cancellationToken)
    {
        var doc = await _repository.GetByIdAsync(request.DocumentId, cancellationToken);
        if (doc is null || string.IsNullOrWhiteSpace(doc.XmlBlobKey))
        {
            return Result.Success<FiscalFileDownloadDto?>(null);
        }

        var streamResult = await _blobStorage.OpenReadAsync(doc.XmlBlobKey, cancellationToken);
        if (streamResult.IsFailure)
        {
            return Result.Failure<FiscalFileDownloadDto?>(streamResult.Error);
        }

        await using var stream = streamResult.Value;
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, cancellationToken);
        return Result.Success<FiscalFileDownloadDto?>(new FiscalFileDownloadDto(
            $"{doc.Id}.xml",
            "application/xml",
            ms.ToArray()));
    }
}

/// <summary>Downloads DANFE PDF when available.</summary>
public sealed class DownloadFiscalDanfeQueryHandler
    : IRequestHandler<DownloadFiscalDanfeQuery, Result<FiscalFileDownloadDto?>>
{
    private readonly IFiscalDocumentRepository _repository;
    private readonly IBlobStorage _blobStorage;

    public DownloadFiscalDanfeQueryHandler(IFiscalDocumentRepository repository, IBlobStorage blobStorage)
    {
        _repository = repository;
        _blobStorage = blobStorage;
    }

    public async Task<Result<FiscalFileDownloadDto?>> Handle(DownloadFiscalDanfeQuery request, CancellationToken cancellationToken)
    {
        var doc = await _repository.GetByIdAsync(request.DocumentId, cancellationToken);
        if (doc is null || string.IsNullOrWhiteSpace(doc.DanfeBlobKey))
        {
            return Result.Success<FiscalFileDownloadDto?>(null);
        }

        var streamResult = await _blobStorage.OpenReadAsync(doc.DanfeBlobKey, cancellationToken);
        if (streamResult.IsFailure)
        {
            return Result.Failure<FiscalFileDownloadDto?>(streamResult.Error);
        }

        await using var stream = streamResult.Value;
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, cancellationToken);
        return Result.Success<FiscalFileDownloadDto?>(new FiscalFileDownloadDto(
            $"{doc.Id}.pdf",
            "application/pdf",
            ms.ToArray()));
    }
}
