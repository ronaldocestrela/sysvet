using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>
/// Published when a clinical quote is approved and queued for PDV conversion (Fase 6 consumer).
/// </summary>
public sealed class ClinicalQuoteApprovedEvent : INotification
{
    public Guid QuoteId { get; }
    public Guid AppointmentId { get; }
    public Guid PetId { get; }
    public Guid TutorId { get; }
    public IReadOnlyList<ClinicalQuoteApprovedItem> Items { get; }

    public ClinicalQuoteApprovedEvent(
        Guid quoteId,
        Guid appointmentId,
        Guid petId,
        Guid tutorId,
        IReadOnlyList<ClinicalQuoteApprovedItem> items)
    {
        QuoteId = quoteId;
        AppointmentId = appointmentId;
        PetId = petId;
        TutorId = tutorId;
        Items = items;
    }
}

/// <summary>Approved quote line snapshot for sales integration.</summary>
public sealed class ClinicalQuoteApprovedItem
{
    public Guid ItemId { get; }
    public string Description { get; }
    public decimal Quantity { get; }
    public decimal UnitPrice { get; }
    public string Kind { get; }
    public Guid? ProductId { get; }

    public ClinicalQuoteApprovedItem(
        Guid itemId,
        string description,
        decimal quantity,
        decimal unitPrice,
        string kind,
        Guid? productId)
    {
        ItemId = itemId;
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Kind = kind;
        ProductId = productId;
    }
}
