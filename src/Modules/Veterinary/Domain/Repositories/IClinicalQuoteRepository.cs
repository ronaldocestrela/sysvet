using Core.Domain;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Enums;

namespace Veterinary.Domain.Repositories;

/// <summary>Persistence port for clinical quotes.</summary>
public interface IClinicalQuoteRepository
{
    Task<ClinicalQuote?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClinicalQuote>> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClinicalQuote>> GetByPetIdAsync(Guid petId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClinicalQuote>> ListPendingConversionsAsync(CancellationToken cancellationToken = default);

    Task AddAsync(ClinicalQuote quote, CancellationToken cancellationToken = default);

    /// <summary>Replaces draft lines after clearing persisted items (avoids EF collection tracking issues).</summary>
    Task<Result> ReplaceDraftItemsAsync(
        Guid quoteId,
        IEnumerable<(Guid ItemId, string Description, decimal Quantity, decimal UnitPrice, ClinicalQuoteItemKind Kind, Guid? ProductId, int SortOrder)> items,
        CancellationToken cancellationToken = default);

    void Update(ClinicalQuote quote);
}
