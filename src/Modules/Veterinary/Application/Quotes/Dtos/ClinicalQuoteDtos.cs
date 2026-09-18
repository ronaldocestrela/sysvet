namespace Veterinary.Application.Quotes.Dtos;

/// <summary>Full clinical quote for detail and print views.</summary>
public sealed class ClinicalQuoteDto
{
    public Guid Id { get; init; }
    public Guid AppointmentId { get; init; }
    public Guid PetId { get; init; }
    public Guid TutorId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string ConversionStatus { get; init; } = string.Empty;
    public Guid? ConvertedOrderId { get; init; }
    public string Notes { get; init; } = string.Empty;
    public DateTimeOffset? SentAt { get; init; }
    public DateTimeOffset? DecidedAt { get; init; }
    public decimal TotalAmount { get; init; }
    public IReadOnlyList<ClinicalQuoteItemDto> Items { get; init; } = Array.Empty<ClinicalQuoteItemDto>();
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>Quote line for API responses.</summary>
public sealed class ClinicalQuoteItemDto
{
    public Guid Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public string Kind { get; init; } = string.Empty;
    public Guid? ProductId { get; init; }
    public int SortOrder { get; init; }
    public decimal LineTotal { get; init; }
}

/// <summary>Summary row for appointment/pet lists.</summary>
public sealed class ClinicalQuoteListItemDto
{
    public Guid Id { get; init; }
    public Guid AppointmentId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string ConversionStatus { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public int ItemCount { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>Approved quote waiting for PDV conversion (Fase 6 inbox).</summary>
public sealed class PendingQuoteConversionDto
{
    public Guid QuoteId { get; init; }
    public Guid AppointmentId { get; init; }
    public Guid PetId { get; init; }
    public string PetName { get; init; } = string.Empty;
    public Guid TutorId { get; init; }
    public string TutorName { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public DateTimeOffset? DecidedAt { get; init; }
    public IReadOnlyList<ClinicalQuoteItemDto> Items { get; init; } = Array.Empty<ClinicalQuoteItemDto>();
}
