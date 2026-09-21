using Core.Domain;
using Inventory.Domain;
using Inventory.Domain.Enums;
using Inventory.Domain.ValueObjects;

namespace Inventory.Domain.Entities;

/// <summary>
/// Purchase NF-e import session from XML upload through stock confirmation.
/// </summary>
public class PurchaseInvoiceImport : AggregateRoot
{
    public string AccessKey { get; private set; } = string.Empty;
    public Guid? SupplierId { get; private set; }
    public string EmitterLegalName { get; private set; } = string.Empty;
    public string EmitterTradeName { get; private set; } = string.Empty;
    public string EmitterDocument { get; private set; } = string.Empty;
    public string InvoiceNumber { get; private set; } = string.Empty;
    public string InvoiceSeries { get; private set; } = string.Empty;
    public DateTimeOffset IssuedAt { get; private set; }
    public decimal TotalAmount { get; private set; }
    public PurchaseImportStatus Status { get; private set; }
    public ApIntegrationStatus ApIntegrationStatus { get; private set; }
    public string BlobKey { get; private set; } = string.Empty;

    /// <summary>Import lines (EF navigation).</summary>
    public List<PurchaseInvoiceImportLine> Lines { get; private set; } = new();

    private PurchaseInvoiceImport() { }

    /// <summary>
    /// Creates a draft import after successful XML parse.
    /// </summary>
    public static Result<PurchaseInvoiceImport> CreateDraft(
        AccessKey accessKey,
        string emitterLegalName,
        string emitterTradeName,
        string emitterDocument,
        string invoiceNumber,
        string invoiceSeries,
        DateTimeOffset issuedAt,
        decimal totalAmount,
        string blobKey,
        IEnumerable<PurchaseInvoiceImportLine> lines,
        Guid? supplierId = null,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(blobKey))
        {
            return Result.Failure<PurchaseInvoiceImport>(ErrorCodes.PurchaseImport.MissingBlobKey);
        }

        var importId = id ?? Guid.NewGuid();
        var import = new PurchaseInvoiceImport
        {
            Id = importId,
            AccessKey = accessKey.Value,
            SupplierId = supplierId,
            EmitterLegalName = emitterLegalName.Trim(),
            EmitterTradeName = emitterTradeName.Trim(),
            EmitterDocument = new string(emitterDocument.Where(char.IsDigit).ToArray()),
            InvoiceNumber = invoiceNumber.Trim(),
            InvoiceSeries = invoiceSeries.Trim(),
            IssuedAt = issuedAt,
            TotalAmount = totalAmount,
            Status = PurchaseImportStatus.Draft,
            ApIntegrationStatus = ApIntegrationStatus.Pending,
            BlobKey = blobKey
        };

        import.Lines.AddRange(lines);

        if (import.Lines.Count == 0)
        {
            return Result.Failure<PurchaseInvoiceImport>(ErrorCodes.PurchaseImport.NoLines);
        }

        return Result.Success(import);
    }

    /// <summary>
    /// Links an existing supplier chosen or matched during preview.
    /// </summary>
    public void AssignSupplier(Guid supplierId) => SupplierId = supplierId;

    /// <summary>
    /// Transitions to confirmed after all lines have products and stock was applied.
    /// </summary>
    public Result Confirm()
    {
        if (Status == PurchaseImportStatus.Confirmed)
        {
            return Result.Failure(ErrorCodes.PurchaseImport.AlreadyConfirmed);
        }

        if (SupplierId is null || SupplierId == Guid.Empty)
        {
            return Result.Failure(ErrorCodes.PurchaseImport.SupplierRequired);
        }

        if (Lines.Any(l => l.ProductId is null))
        {
            return Result.Failure(ErrorCodes.PurchaseImport.LineWithoutProduct);
        }

        if (Lines.Any(l => l.StockMovementId is null))
        {
            return Result.Failure(ErrorCodes.PurchaseImport.StockNotApplied);
        }

        Status = PurchaseImportStatus.Confirmed;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Marks accounts payable linkage complete after Finance creates payable titles.
    /// </summary>
    public Result MarkApLinked()
    {
        if (ApIntegrationStatus == ApIntegrationStatus.Linked)
        {
            return Result.Success();
        }

        if (Status != PurchaseImportStatus.Confirmed)
        {
            return Result.Failure(ErrorCodes.PurchaseImport.ApLinkNotAllowed);
        }

        ApIntegrationStatus = ApIntegrationStatus.Linked;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
