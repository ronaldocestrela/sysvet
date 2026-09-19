using Core.Application.Storage;
using Core.Domain;
using Core.Domain.Auditing;
using Inventory.Application.PurchaseImports.Dtos;
using Inventory.Domain.Entities;
using PurchaseImportErrors = Inventory.Domain.ErrorCodes.PurchaseImport;
using Inventory.Domain.Repositories;
using MediatR;

namespace Inventory.Application.PurchaseImports.Commands;

/// <summary>Parses NF-e XML, stores blob, and builds a draft import with match hints.</summary>
public sealed class ParsePurchaseNfeXmlCommandHandler : IRequestHandler<ParsePurchaseNfeXmlCommand, Result<PurchaseImportPreviewDto>>
{
    private const long MaxBytes = 2 * 1024 * 1024;

    private readonly IPurchaseInvoiceImportRepository _importRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IProductRepository _productRepository;
    private readonly ISupplierProductMappingRepository _mappingRepository;
    private readonly IProductLotRepository _lotRepository;
    private readonly IBlobStorage _blobStorage;
    private readonly ITenantContext _tenantContext;
    private readonly IAuditLogger _auditLogger;

    public ParsePurchaseNfeXmlCommandHandler(
        IPurchaseInvoiceImportRepository importRepository,
        ISupplierRepository supplierRepository,
        IProductRepository productRepository,
        ISupplierProductMappingRepository mappingRepository,
        IProductLotRepository lotRepository,
        IBlobStorage blobStorage,
        ITenantContext tenantContext,
        IAuditLogger auditLogger)
    {
        _importRepository = importRepository;
        _supplierRepository = supplierRepository;
        _productRepository = productRepository;
        _mappingRepository = mappingRepository;
        _lotRepository = lotRepository;
        _blobStorage = blobStorage;
        _tenantContext = tenantContext;
        _auditLogger = auditLogger;
    }

    public async Task<Result<PurchaseImportPreviewDto>> Handle(ParsePurchaseNfeXmlCommand request, CancellationToken cancellationToken)
    {
        if (request.ContentLength <= 0 || request.ContentLength > MaxBytes)
        {
            return Result.Failure<PurchaseImportPreviewDto>(PurchaseImportErrors.FileTooLarge);
        }

        if (!IsXmlContent(request.ContentType, request.FileName))
        {
            return Result.Failure<PurchaseImportPreviewDto>(PurchaseImportErrors.InvalidContentType);
        }

        await using var buffer = new MemoryStream();
        await request.Content.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;

        var parseResult = NfePurchaseXmlParser.Parse(buffer);
        if (parseResult.IsFailure)
        {
            return Result.Failure<PurchaseImportPreviewDto>(parseResult.Error);
        }

        var parsed = parseResult.Value;
        var existing = await _importRepository.GetByAccessKeyAsync(parsed.AccessKey.Value, cancellationToken);
        if (existing?.Status == Domain.Enums.PurchaseImportStatus.Confirmed)
        {
            return Result.Failure<PurchaseImportPreviewDto>(PurchaseImportErrors.AccessKeyConflict);
        }

        if (existing is not null)
        {
            return Result.Success(await BuildPreviewAsync(existing, cancellationToken));
        }

        var importId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var blobKey = $"{_tenantContext.SchemaName}/purchases/{now:yyyy}/{now:MM}/{importId}.xml";

        buffer.Position = 0;
        var put = await _blobStorage.PutAsync(blobKey, buffer, "application/xml", cancellationToken);
        if (put.IsFailure)
        {
            return Result.Failure<PurchaseImportPreviewDto>(put.Error);
        }

        Supplier? matchedSupplier = await _supplierRepository.GetByDocumentAsync(parsed.EmitterDocument, cancellationToken);

        var lines = parsed.Lines.Select(l =>
            PurchaseInvoiceImportLine.Create(
                importId,
                l.ItemNumber,
                l.SupplierProductCode,
                l.Barcode,
                l.Description,
                l.Ncm,
                l.UnitOfMeasure,
                l.Quantity,
                l.UnitCost,
                l.LineTotal,
                l.LotNumber,
                l.ExpirationDate)).ToList();

        var draft = PurchaseInvoiceImport.CreateDraft(
            parsed.AccessKey,
            parsed.EmitterLegalName,
            parsed.EmitterTradeName,
            parsed.EmitterDocument,
            parsed.InvoiceNumber,
            parsed.InvoiceSeries,
            parsed.IssuedAt,
            parsed.TotalAmount,
            blobKey,
            lines,
            matchedSupplier?.Id,
            importId);

        if (draft.IsFailure)
        {
            await _blobStorage.DeleteAsync(blobKey, cancellationToken);
            return Result.Failure<PurchaseImportPreviewDto>(draft.Error);
        }

        _importRepository.Add(draft.Value);

        await _auditLogger.LogAsync(
            _tenantContext.TenantId,
            _tenantContext.UserId,
            draft.Value.Id,
            "PurchaseInvoiceImport",
            "Parse",
            $"NF-e {parsed.AccessKey.Value}",
            cancellationToken);

        return Result.Success(await BuildPreviewAsync(draft.Value, cancellationToken, matchedSupplier?.Id));
    }

    private async Task<PurchaseImportPreviewDto> BuildPreviewAsync(
        PurchaseInvoiceImport import,
        CancellationToken cancellationToken,
        Guid? matchedSupplierId = null)
    {
        matchedSupplierId ??= import.SupplierId;
        Supplier? supplier = matchedSupplierId is Guid sid
            ? await _supplierRepository.GetByIdAsync(sid, cancellationToken)
            : await _supplierRepository.GetByDocumentAsync(import.EmitterDocument, cancellationToken);

        var linePreviews = new List<PurchaseImportLinePreviewDto>();
        foreach (var line in import.Lines)
        {
            Guid? suggested = line.ProductId;
            if (suggested is null && supplier is not null)
            {
                var mapping = await _mappingRepository.GetBySupplierAndCodeAsync(supplier.Id, line.SupplierProductCode, cancellationToken);
                if (mapping is not null)
                {
                    suggested = mapping.ProductId;
                }
            }

            if (suggested is null && !string.IsNullOrWhiteSpace(line.Barcode))
            {
                suggested = (await _productRepository.GetByBarcodeAsync(line.Barcode, cancellationToken))?.Id;
            }

            if (suggested is null)
            {
                var sku = PurchaseImportSkuHelper.SanitizeSku(line.SupplierProductCode);
                suggested = (await _productRepository.GetBySkuAsync(sku, cancellationToken))?.Id;
            }

            linePreviews.Add(new PurchaseImportLinePreviewDto
            {
                LineId = line.Id,
                ItemNumber = line.ItemNumber,
                SupplierProductCode = line.SupplierProductCode,
                Barcode = line.Barcode,
                Description = line.Description,
                Ncm = line.Ncm,
                UnitOfMeasure = line.UnitOfMeasure,
                Quantity = line.Quantity,
                UnitCost = line.UnitCost,
                LotNumber = line.LotNumber,
                ExpirationDate = line.ExpirationDate,
                SuggestedProductId = suggested,
                SuggestedSku = PurchaseImportSkuHelper.SanitizeSku(line.SupplierProductCode),
                RequiresMapping = suggested is null
            });
        }

        return new PurchaseImportPreviewDto
        {
            ImportId = import.Id,
            AccessKey = import.AccessKey,
            InvoiceNumber = import.InvoiceNumber,
            InvoiceSeries = import.InvoiceSeries,
            IssuedAt = import.IssuedAt,
            TotalAmount = import.TotalAmount,
            MatchedSupplierId = supplier?.Id,
            Emitter = new PurchaseImportSupplierPreviewDto
            {
                LegalName = import.EmitterLegalName,
                TradeName = import.EmitterTradeName,
                Document = import.EmitterDocument
            },
            Lines = linePreviews
        };
    }

    private static bool IsXmlContent(string contentType, string fileName)
    {
        if (fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return contentType.Contains("xml", StringComparison.OrdinalIgnoreCase)
               || string.Equals(contentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase);
    }
}
