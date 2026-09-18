using Core.Domain;
using Microsoft.EntityFrameworkCore;
using VetErrors = Veterinary.Domain.ErrorCodes;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Enums;
using Veterinary.Domain.Repositories;

namespace Veterinary.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of clinical quote persistence.</summary>
public sealed class ClinicalQuoteRepository : IClinicalQuoteRepository
{
    private readonly VeterinaryDbContext _dbContext;

    public ClinicalQuoteRepository(VeterinaryDbContext dbContext) => _dbContext = dbContext;

    public async Task AddAsync(ClinicalQuote quote, CancellationToken cancellationToken = default) =>
        await _dbContext.ClinicalQuotes.AddAsync(quote, cancellationToken);

    public async Task<ClinicalQuote?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.ClinicalQuotes.Include(q => q.Items).FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ClinicalQuote>> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        var quotes = await _dbContext.ClinicalQuotes.AsNoTracking()
            .Include(q => q.Items)
            .Where(q => q.AppointmentId == appointmentId)
            .ToListAsync(cancellationToken);
        return quotes.OrderByDescending(q => q.UpdatedAt).ToList();
    }

    public async Task<IReadOnlyList<ClinicalQuote>> GetByPetIdAsync(Guid petId, CancellationToken cancellationToken = default)
    {
        var quotes = await _dbContext.ClinicalQuotes.AsNoTracking()
            .Include(q => q.Items)
            .Where(q => q.PetId == petId)
            .ToListAsync(cancellationToken);
        return quotes.OrderByDescending(q => q.UpdatedAt).ToList();
    }

    public async Task<IReadOnlyList<ClinicalQuote>> ListPendingConversionsAsync(CancellationToken cancellationToken = default)
    {
        var quotes = await _dbContext.ClinicalQuotes.AsNoTracking()
            .Include(q => q.Items)
            .Where(q => q.ConversionStatus == QuoteConversionStatus.Pending)
            .ToListAsync(cancellationToken);
        return quotes.OrderByDescending(q => q.DecidedAt).ToList();
    }

    public async Task<Result> ReplaceDraftItemsAsync(
        Guid quoteId,
        IEnumerable<(Guid ItemId, string Description, decimal Quantity, decimal UnitPrice, ClinicalQuoteItemKind Kind, Guid? ProductId, int SortOrder)> items,
        CancellationToken cancellationToken = default)
    {
        var quote = await GetByIdAsync(quoteId, cancellationToken);
        if (quote is null)
        {
            return Result.Failure(VetErrors.ClinicalQuote.NotFound);
        }

        await _dbContext.ClinicalQuoteItems
            .Where(i => i.ClinicalQuoteId == quoteId)
            .ExecuteDeleteAsync(cancellationToken);

        foreach (var entry in _dbContext.ChangeTracker.Entries<ClinicalQuoteItem>()
                     .Where(e => e.Entity.ClinicalQuoteId == quoteId)
                     .ToList())
        {
            entry.State = EntityState.Detached;
        }

        var replace = quote.ReplaceDraftItems(items);
        if (replace.IsFailure)
        {
            return replace;
        }

        foreach (var item in quote.Items)
        {
            await _dbContext.ClinicalQuoteItems.AddAsync(item, cancellationToken);
        }

        _dbContext.ClinicalQuotes.Update(quote);
        return Result.Success();
    }

    public void Update(ClinicalQuote quote) => _dbContext.ClinicalQuotes.Update(quote);
}
