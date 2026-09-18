using Core.Domain;

namespace Clients.Infrastructure.Crm;

/// <summary>Offline-first clinical quotes.</summary>
public interface IClinicalQuoteStore
{
    Task<Result<IReadOnlyList<ClinicalQuoteListItemDto>>> GetByAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    Task<Result<ClinicalQuoteDetailDto>> GetByIdAsync(Guid quoteId, CancellationToken cancellationToken = default);

    Task<Result<Guid>> CreateDraftAsync(Guid appointmentId, string? notes, CancellationToken cancellationToken = default);

    Task<Result> ReplaceItemsAsync(Guid quoteId, IReadOnlyList<ClinicalQuoteLineDto> items, string? notes, CancellationToken cancellationToken = default);

    Task<Result> SendAsync(Guid quoteId, CancellationToken cancellationToken = default);

    Task<Result> ApproveAsync(Guid quoteId, CancellationToken cancellationToken = default);

    Task<Result> RejectAsync(Guid quoteId, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<PendingQuoteConversionListItemDto>>> ListPendingConversionsAsync(CancellationToken cancellationToken = default);
}

public sealed class ClinicalQuoteListItemDto
{
    public Guid Id { get; init; }
    public string Status { get; init; } = string.Empty;
    public string ConversionStatus { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public int ItemCount { get; init; }
}

public sealed class ClinicalQuoteDetailDto
{
    public Guid Id { get; init; }
    public Guid AppointmentId { get; init; }
    public Guid PetId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public IReadOnlyList<ClinicalQuoteLineDto> Items { get; init; } = Array.Empty<ClinicalQuoteLineDto>();
}

public sealed class ClinicalQuoteLineDto
{
    public Guid Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public string Kind { get; init; } = string.Empty;
    public int SortOrder { get; init; }
}

public sealed class PendingQuoteConversionListItemDto
{
    public Guid QuoteId { get; init; }
    public Guid PetId { get; init; }
    public string PetName { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public DateTimeOffset? DecidedAt { get; init; }
}
