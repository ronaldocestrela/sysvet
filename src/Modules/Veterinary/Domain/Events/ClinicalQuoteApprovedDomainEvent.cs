using Core.Domain;
using Veterinary.Domain.Enums;

namespace Veterinary.Domain.Events;

/// <summary>
/// Raised when a clinical quote is approved and queued for future PDV conversion.
/// </summary>
public sealed record ClinicalQuoteApprovedDomainEvent(
    Guid QuoteId,
    Guid AppointmentId,
    Guid PetId,
    Guid TutorId,
    IReadOnlyList<ClinicalQuoteApprovedLine> Lines,
    DateTimeOffset OccurredOn) : IDomainEvent;

/// <summary>Snapshot of one approved quote line for integration consumers.</summary>
public sealed record ClinicalQuoteApprovedLine(
    Guid ItemId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    ClinicalQuoteItemKind Kind,
    Guid? ProductId);
