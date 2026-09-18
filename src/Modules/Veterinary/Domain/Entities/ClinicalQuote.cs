using Core.Domain;
using Veterinary.Domain.Enums;
using Veterinary.Domain.Events;

namespace Veterinary.Domain.Entities;

/// <summary>Clinical budget linked to a consultation, with PDV conversion queue on approval.</summary>
public sealed class ClinicalQuote : AggregateRoot
{
    private readonly List<ClinicalQuoteItem> _items = new();

    /// <summary>Consultation appointment.</summary>
    public Guid AppointmentId { get; private set; }

    /// <summary>Patient pet.</summary>
    public Guid PetId { get; private set; }

    /// <summary>Pet owner for commercial context.</summary>
    public Guid TutorId { get; private set; }

    /// <summary>Staff user who created the quote.</summary>
    public Guid CreatedByUserId { get; private set; }

    /// <summary>Workflow status.</summary>
    public ClinicalQuoteStatus Status { get; private set; }

    /// <summary>PDV conversion tracking.</summary>
    public QuoteConversionStatus ConversionStatus { get; private set; }

    /// <summary>Linked order after Fase 6 conversion.</summary>
    public Guid? ConvertedOrderId { get; private set; }

    /// <summary>Optional notes visible on print.</summary>
    public string Notes { get; private set; } = string.Empty;

    /// <summary>When the quote was sent to the tutor.</summary>
    public DateTimeOffset? SentAt { get; private set; }

    /// <summary>When the tutor approved or rejected.</summary>
    public DateTimeOffset? DecidedAt { get; private set; }

    /// <summary>Quote lines.</summary>
    public IReadOnlyCollection<ClinicalQuoteItem> Items => _items.AsReadOnly();

    /// <summary>Sum of line totals in BRL.</summary>
    public decimal TotalAmount => _items.Sum(i => i.LineTotal);

    private ClinicalQuote() { }

    private ClinicalQuote(
        Guid id,
        Guid appointmentId,
        Guid petId,
        Guid tutorId,
        Guid createdByUserId)
        : base(id)
    {
        AppointmentId = appointmentId;
        PetId = petId;
        TutorId = tutorId;
        CreatedByUserId = createdByUserId;
        Status = ClinicalQuoteStatus.Draft;
        ConversionStatus = QuoteConversionStatus.None;
    }

    /// <summary>Creates a new draft quote for a visit.</summary>
    public static Result<ClinicalQuote> Create(
        Guid id,
        Guid appointmentId,
        Guid petId,
        Guid tutorId,
        Guid createdByUserId)
    {
        if (appointmentId == Guid.Empty || petId == Guid.Empty || tutorId == Guid.Empty || createdByUserId == Guid.Empty)
        {
            return Result.Failure<ClinicalQuote>(ErrorCodes.ClinicalQuote.InvalidIdentifiers);
        }

        var quote = new ClinicalQuote(id, appointmentId, petId, tutorId, createdByUserId);
        quote.Touch();
        return Result.Success(quote);
    }

    /// <summary>Rehydrates from sync pull.</summary>
    public static ClinicalQuote RestoreFromSync(
        Guid id,
        Guid appointmentId,
        Guid petId,
        Guid tutorId,
        Guid createdByUserId,
        ClinicalQuoteStatus status,
        QuoteConversionStatus conversionStatus,
        Guid? convertedOrderId,
        string notes,
        DateTimeOffset? sentAt,
        DateTimeOffset? decidedAt,
        DateTimeOffset updatedAt,
        IEnumerable<(Guid ItemId, string Description, decimal Quantity, decimal UnitPrice, ClinicalQuoteItemKind Kind, Guid? ProductId, int SortOrder)> items)
    {
        var quote = new ClinicalQuote(id, appointmentId, petId, tutorId, createdByUserId)
        {
            Status = status,
            ConversionStatus = conversionStatus,
            ConvertedOrderId = convertedOrderId,
            Notes = notes,
            SentAt = sentAt,
            DecidedAt = decidedAt,
            UpdatedAt = updatedAt
        };

        foreach (var item in items)
        {
            var created = ClinicalQuoteItem.Create(
                item.ItemId,
                id,
                item.Description,
                item.Quantity,
                item.UnitPrice,
                item.Kind,
                item.ProductId,
                item.SortOrder);
            if (created.IsSuccess)
            {
                quote._items.Add(created.Value);
            }
        }

        return quote;
    }

    /// <summary>Applies remote sync snapshot; never downgrades terminal statuses.</summary>
    public void ApplySyncSnapshot(
        Guid appointmentId,
        Guid petId,
        Guid tutorId,
        Guid createdByUserId,
        ClinicalQuoteStatus status,
        QuoteConversionStatus conversionStatus,
        Guid? convertedOrderId,
        string notes,
        DateTimeOffset? sentAt,
        DateTimeOffset? decidedAt,
        DateTimeOffset updatedAt,
        IEnumerable<(Guid ItemId, string Description, decimal Quantity, decimal UnitPrice, ClinicalQuoteItemKind Kind, Guid? ProductId, int SortOrder)> items)
    {
        if (Status is ClinicalQuoteStatus.Approved or ClinicalQuoteStatus.Rejected
            && status is ClinicalQuoteStatus.Draft or ClinicalQuoteStatus.Sent)
        {
            status = Status;
            conversionStatus = ConversionStatus;
            convertedOrderId = ConvertedOrderId;
        }

        if (Status == ClinicalQuoteStatus.Approved
            && status == ClinicalQuoteStatus.Approved
            && ConversionStatus == QuoteConversionStatus.Pending
            && conversionStatus == QuoteConversionStatus.None)
        {
            conversionStatus = ConversionStatus;
        }

        if (ConversionStatus == QuoteConversionStatus.Converted && conversionStatus != QuoteConversionStatus.Converted)
        {
            conversionStatus = ConversionStatus;
            convertedOrderId = ConvertedOrderId;
        }

        AppointmentId = appointmentId;
        PetId = petId;
        TutorId = tutorId;
        CreatedByUserId = createdByUserId;
        Status = status;
        ConversionStatus = conversionStatus;
        ConvertedOrderId = convertedOrderId;
        Notes = notes;
        SentAt = sentAt;
        DecidedAt = decidedAt;
        UpdatedAt = updatedAt;
        _items.Clear();

        foreach (var item in items)
        {
            var created = ClinicalQuoteItem.Create(
                item.ItemId,
                Id,
                item.Description,
                item.Quantity,
                item.UnitPrice,
                item.Kind,
                item.ProductId,
                item.SortOrder);
            if (created.IsSuccess)
            {
                _items.Add(created.Value);
            }
        }
    }

    /// <summary>Updates draft notes.</summary>
    public Result UpdateNotes(string? notes)
    {
        if (!EnsureDraft())
        {
            return Result.Failure(ErrorCodes.ClinicalQuote.InvalidTransition);
        }

        var value = notes?.Trim() ?? string.Empty;
        if (value.Length > 2000)
        {
            return Result.Failure(ErrorCodes.ClinicalQuote.InvalidNotes);
        }

        Notes = value;
        Touch();
        return Result.Success();
    }

    /// <summary>Replaces draft line items.</summary>
    public Result ReplaceDraftItems(
        IEnumerable<(Guid ItemId, string Description, decimal Quantity, decimal UnitPrice, ClinicalQuoteItemKind Kind, Guid? ProductId, int SortOrder)> items)
    {
        if (!EnsureDraft())
        {
            return Result.Failure(ErrorCodes.ClinicalQuote.InvalidTransition);
        }

        _items.Clear();
        var order = 0;
        foreach (var item in items)
        {
            var created = ClinicalQuoteItem.Create(
                item.ItemId == Guid.Empty ? Guid.NewGuid() : item.ItemId,
                Id,
                item.Description,
                item.Quantity,
                item.UnitPrice,
                item.Kind,
                item.ProductId,
                item.SortOrder == 0 ? order++ : item.SortOrder);
            if (created.IsFailure)
            {
                return Result.Failure(created.Error);
            }

            _items.Add(created.Value);
        }

        Touch();
        return Result.Success();
    }

    /// <summary>Marks the quote as sent to the tutor.</summary>
    public Result Send()
    {
        if (!EnsureDraft())
        {
            return Result.Failure(ErrorCodes.ClinicalQuote.InvalidTransition);
        }

        if (_items.Count == 0)
        {
            return Result.Failure(ErrorCodes.ClinicalQuote.EmptyItems);
        }

        Status = ClinicalQuoteStatus.Sent;
        SentAt = DateTimeOffset.UtcNow;
        Touch();
        return Result.Success();
    }

    /// <summary>Records tutor approval and queues PDV conversion.</summary>
    public Result Approve()
    {
        if (Status != ClinicalQuoteStatus.Sent)
        {
            return Result.Failure(ErrorCodes.ClinicalQuote.InvalidTransition);
        }

        Status = ClinicalQuoteStatus.Approved;
        ConversionStatus = QuoteConversionStatus.Pending;
        DecidedAt = DateTimeOffset.UtcNow;
        Touch();

        var lines = _items
            .OrderBy(i => i.SortOrder)
            .Select(i => new ClinicalQuoteApprovedLine(
                i.Id,
                i.Description,
                i.Quantity,
                i.UnitPrice,
                i.Kind,
                i.ProductId))
            .ToList();

        Raise(new ClinicalQuoteApprovedDomainEvent(
            Id,
            AppointmentId,
            PetId,
            TutorId,
            lines,
            DateTimeOffset.UtcNow));

        return Result.Success();
    }

    /// <summary>Records tutor rejection.</summary>
    public Result Reject()
    {
        if (Status != ClinicalQuoteStatus.Sent)
        {
            return Result.Failure(ErrorCodes.ClinicalQuote.InvalidTransition);
        }

        Status = ClinicalQuoteStatus.Rejected;
        ConversionStatus = QuoteConversionStatus.None;
        DecidedAt = DateTimeOffset.UtcNow;
        Touch();
        return Result.Success();
    }

    /// <summary>Marks conversion complete (Fase 6).</summary>
    public Result MarkConverted(Guid orderId)
    {
        if (Status != ClinicalQuoteStatus.Approved || ConversionStatus != QuoteConversionStatus.Pending)
        {
            return Result.Failure(ErrorCodes.ClinicalQuote.InvalidTransition);
        }

        if (orderId == Guid.Empty)
        {
            return Result.Failure(ErrorCodes.ClinicalQuote.InvalidIdentifiers);
        }

        ConversionStatus = QuoteConversionStatus.Converted;
        ConvertedOrderId = orderId;
        Touch();
        return Result.Success();
    }

    private bool EnsureDraft() => Status == ClinicalQuoteStatus.Draft;

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
