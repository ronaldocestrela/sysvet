using Core.Domain;
using Fiscal.Domain.Enums;

namespace Fiscal.Domain.Entities;

/// <summary>Outbound NF-e or NFS-e fiscal document aggregate.</summary>
public sealed class FiscalDocument : AggregateRoot
{
    public FiscalDocumentType DocumentType { get; private set; }
    public FiscalDocumentStatus Status { get; private set; } = FiscalDocumentStatus.Draft;
    public FiscalSourceType SourceType { get; private set; }
    public Guid SourceOrderId { get; private set; }
    public string? AccessKey { get; private set; }
    public string? Protocol { get; private set; }
    public string? RejectionReason { get; private set; }
    public string? XmlBlobKey { get; private set; }
    public string? DanfeBlobKey { get; private set; }
    public int? NfeNumber { get; private set; }
    public int? NfeSeries { get; private set; }
    public string? NfseNumber { get; private set; }
    public DateTimeOffset? AuthorizedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public string RecipientName { get; private set; } = string.Empty;
    public string? RecipientCpf { get; private set; }
    public string? RecipientUf { get; private set; }
    public FiscalEmissionType? EmissionType { get; private set; }
    public string? QrCodeUrl { get; private set; }

    private readonly List<FiscalDocumentItem> _items = new();
    public IReadOnlyCollection<FiscalDocumentItem> Items => _items.AsReadOnly();

    private readonly List<FiscalCorrectionLetter> _corrections = new();
    public IReadOnlyCollection<FiscalCorrectionLetter> Corrections => _corrections.AsReadOnly();

    public decimal TotalAmount => _items.Sum(i => i.Total);

    private FiscalDocument() { }

    /// <summary>Starts a draft document from a paid sale.</summary>
    public static Result<FiscalDocument> CreateDraft(
        FiscalDocumentType type,
        Guid orderId,
        string recipientName,
        string? recipientCpf,
        string? recipientUf,
        Guid? id = null)
    {
        if (orderId == Guid.Empty)
        {
            return Result.Failure<FiscalDocument>(ErrorCodes.Document.OrderNotPaid);
        }

        if (string.IsNullOrWhiteSpace(recipientName))
        {
            recipientName = "Consumidor";
        }

        var doc = new FiscalDocument
        {
            Id = id ?? Guid.NewGuid(),
            DocumentType = type,
            SourceType = FiscalSourceType.Sale,
            SourceOrderId = orderId,
            RecipientName = recipientName.Trim(),
            RecipientCpf = string.IsNullOrWhiteSpace(recipientCpf) ? null : new string(recipientCpf.Where(char.IsDigit).ToArray()),
            RecipientUf = string.IsNullOrWhiteSpace(recipientUf) ? null : recipientUf.Trim().ToUpperInvariant()
        };

        return Result.Success(doc);
    }

    /// <summary>Replaces recipient copy fields after tutor anonymization (XML blobs unchanged).</summary>
    public void ReplaceRecipientCopy(string recipientName, string? recipientCpf)
    {
        RecipientName = recipientName.Trim();
        RecipientCpf = string.IsNullOrWhiteSpace(recipientCpf)
            ? null
            : new string(recipientCpf.Where(char.IsDigit).ToArray());
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Adds a product line to an NF-e draft.</summary>
    public Result AddProductItem(
        Guid? productId,
        string description,
        decimal quantity,
        decimal unitPrice,
        string ncm,
        string cfop,
        string csosn,
        int merchandiseOrigin)
    {
        if (DocumentType is not (FiscalDocumentType.Nfe or FiscalDocumentType.Nfce))
        {
            return Result.Failure(ErrorCodes.Document.InvalidTransition);
        }

        if (Status != FiscalDocumentStatus.Draft)
        {
            return Result.Failure(ErrorCodes.Document.InvalidTransition);
        }

        _items.Add(FiscalDocumentItem.CreateProductLine(
            Id, productId, description, quantity, unitPrice, ncm, cfop, csosn, merchandiseOrigin));
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Adds a service line to an NFS-e draft.</summary>
    public Result AddServiceItem(string description, decimal quantity, decimal unitPrice)
    {
        if (DocumentType != FiscalDocumentType.Nfse)
        {
            return Result.Failure(ErrorCodes.Document.InvalidTransition);
        }

        if (Status != FiscalDocumentStatus.Draft)
        {
            return Result.Failure(ErrorCodes.Document.InvalidTransition);
        }

        _items.Add(FiscalDocumentItem.CreateServiceLine(Id, description, quantity, unitPrice));
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Marks document as transmitting to authority.</summary>
    public Result BeginTransmission()
    {
        if (Status is not (FiscalDocumentStatus.Draft or FiscalDocumentStatus.Rejected or FiscalDocumentStatus.ContingencyIssued))
        {
            return Result.Failure(ErrorCodes.Document.InvalidTransition);
        }

        if (_items.Count == 0)
        {
            return Result.Failure(ErrorCodes.Document.NoLines);
        }

        Status = FiscalDocumentStatus.Transmitting;
        RejectionReason = null;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Records NFC-e signed in contingency (tpEmis=9) before SEFAZ transmission.</summary>
    public Result MarkContingencyIssued(
        string accessKey,
        string xmlBlobKey,
        string? qrCodeUrl,
        int nfeNumber,
        int nfeSeries,
        FiscalEmissionType emissionType)
    {
        if (DocumentType != FiscalDocumentType.Nfce)
        {
            return Result.Failure(ErrorCodes.Document.InvalidTransition);
        }

        if (Status != FiscalDocumentStatus.Draft)
        {
            return Result.Failure(ErrorCodes.Document.InvalidTransition);
        }

        if (_items.Count == 0)
        {
            return Result.Failure(ErrorCodes.Document.NoLines);
        }

        var keyResult = ValueObjects.NfeAccessKey.CreateForNfce(accessKey);
        if (keyResult.IsFailure)
        {
            return Result.Failure(keyResult.Error);
        }

        AccessKey = keyResult.Value.Value;
        XmlBlobKey = xmlBlobKey;
        QrCodeUrl = qrCodeUrl;
        NfeNumber = nfeNumber;
        NfeSeries = nfeSeries;
        EmissionType = emissionType;
        Status = FiscalDocumentStatus.ContingencyIssued;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Records SEFAZ/ADN authorization.</summary>
    public Result MarkAuthorized(
        string? accessKey,
        string protocol,
        string xmlBlobKey,
        string? danfeBlobKey,
        int? nfeNumber,
        int? nfeSeries,
        string? nfseNumber)
    {
        if (Status != FiscalDocumentStatus.Transmitting)
        {
            return Result.Failure(ErrorCodes.Document.InvalidTransition);
        }

        AccessKey = accessKey;
        Protocol = protocol;
        XmlBlobKey = xmlBlobKey;
        DanfeBlobKey = danfeBlobKey;
        NfeNumber = nfeNumber;
        NfeSeries = nfeSeries;
        NfseNumber = nfseNumber;
        Status = FiscalDocumentStatus.Authorized;
        AuthorizedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Records authority rejection.</summary>
    public Result MarkRejected(string reason)
    {
        if (Status != FiscalDocumentStatus.Transmitting)
        {
            return Result.Failure(ErrorCodes.Document.InvalidTransition);
        }

        Status = FiscalDocumentStatus.Rejected;
        RejectionReason = reason;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Cancels an authorized document within policy window.</summary>
    public Result Cancel(TimeSpan maxWindowSinceAuthorization)
    {
        if (Status != FiscalDocumentStatus.Authorized)
        {
            return Result.Failure(ErrorCodes.Document.InvalidTransition);
        }

        if (AuthorizedAt is null || DateTimeOffset.UtcNow - AuthorizedAt.Value > maxWindowSinceAuthorization)
        {
            return Result.Failure(ErrorCodes.Document.CancelWindowExpired);
        }

        Status = FiscalDocumentStatus.Cancelled;
        CancelledAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Registers a CC-e for authorized NF-e.</summary>
    public Result<FiscalCorrectionLetter> AddCorrectionLetter(string correctionText)
    {
        if (DocumentType != FiscalDocumentType.Nfe || Status != FiscalDocumentStatus.Authorized)
        {
            return Result.Failure<FiscalCorrectionLetter>(ErrorCodes.Document.CorrectionNotAllowed);
        }

        var sequence = _corrections.Count + 1;
        var letterResult = FiscalCorrectionLetter.Create(Id, sequence, correctionText);
        if (letterResult.IsFailure)
        {
            return letterResult;
        }

        _corrections.Add(letterResult.Value);
        UpdatedAt = DateTimeOffset.UtcNow;
        return letterResult;
    }
}
