using Clients.Infrastructure.Sync;
using Core.Domain;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Enums;

namespace Clients.Infrastructure.Crm;

/// <summary>SQLite-backed clinical quote store with sync outbox.</summary>
public sealed class OfflineClinicalQuoteStore : IClinicalQuoteStore
{
    private readonly OfflineDbContext _dbContext;

    public OfflineClinicalQuoteStore(OfflineDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<IReadOnlyList<ClinicalQuoteListItemDto>>> GetByAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        var quotes = await _dbContext.ClinicalQuotes.AsNoTracking()
            .Include(q => q.Items)
            .Where(q => q.AppointmentId == appointmentId)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<ClinicalQuoteListItemDto>>(quotes
            .OrderByDescending(q => q.UpdatedAt)
            .Select(MapListItem)
            .ToList());
    }

    public async Task<Result<ClinicalQuoteDetailDto>> GetByIdAsync(Guid quoteId, CancellationToken cancellationToken = default)
    {
        var quote = await _dbContext.ClinicalQuotes.AsNoTracking()
            .Include(q => q.Items)
            .FirstOrDefaultAsync(q => q.Id == quoteId, cancellationToken);
        return quote is null
            ? Result.Failure<ClinicalQuoteDetailDto>(new Error("ClinicalQuote.NotFound", "Quote not found locally."))
            : Result.Success(MapDetail(quote));
    }

    public async Task<Result<Guid>> CreateDraftAsync(Guid appointmentId, string? notes, CancellationToken cancellationToken = default)
    {
        var appointment = await _dbContext.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId, cancellationToken);
        if (appointment is null)
        {
            return Result.Failure<Guid>(new Error("ClinicalQuote.AppointmentNotFound", "Appointment not found locally."));
        }

        var id = Guid.NewGuid();
        var created = ClinicalQuote.Create(id, appointmentId, appointment.PetId, appointment.TutorId, Guid.Empty);
        if (created.IsFailure)
        {
            return Result.Failure<Guid>(created.Error);
        }

        if (!string.IsNullOrWhiteSpace(notes))
        {
            created.Value.UpdateNotes(notes);
        }

        _dbContext.ClinicalQuotes.Add(created.Value);
        EnqueueOutbox("CreateClinicalQuoteCommand", OutboxPayloadFactory.CreateClinicalQuote(appointmentId, notes, id, id));
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(id);
    }

    public async Task<Result> ReplaceItemsAsync(Guid quoteId, IReadOnlyList<ClinicalQuoteLineDto> items, string? notes, CancellationToken cancellationToken = default)
    {
        var quote = await _dbContext.ClinicalQuotes.Include(q => q.Items).FirstOrDefaultAsync(q => q.Id == quoteId, cancellationToken);
        if (quote is null)
        {
            return Result.Failure(new Error("ClinicalQuote.NotFound", "Quote not found locally."));
        }

        var lines = items.Select(i =>
        {
            Enum.TryParse<ClinicalQuoteItemKind>(i.Kind, true, out var kind);
            return (i.Id == Guid.Empty ? Guid.NewGuid() : i.Id, i.Description, i.Quantity, i.UnitPrice, kind, (Guid?)null, i.SortOrder);
        }).ToList();

        var replace = quote.ReplaceDraftItems(lines);
        if (replace.IsFailure)
        {
            return replace;
        }

        if (notes is not null)
        {
            var noteResult = quote.UpdateNotes(notes);
            if (noteResult.IsFailure)
            {
                return noteResult;
            }
        }

        _dbContext.ClinicalQuoteItems.RemoveRange(_dbContext.ClinicalQuoteItems.Where(i => i.ClinicalQuoteId == quoteId));
        foreach (var item in quote.Items)
        {
            await _dbContext.ClinicalQuoteItems.AddAsync(item, cancellationToken);
        }

        EnqueueOutbox("ReplaceClinicalQuoteItemsCommand", OutboxPayloadFactory.ReplaceClinicalQuoteItems(quoteId, items, notes, Guid.NewGuid()));
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> SendAsync(Guid quoteId, CancellationToken cancellationToken = default)
    {
        return await TransitionAsync(quoteId, q => q.Send(), "SendClinicalQuoteCommand", OutboxPayloadFactory.SendClinicalQuote(quoteId, Guid.NewGuid()), cancellationToken);
    }

    public async Task<Result> ApproveAsync(Guid quoteId, CancellationToken cancellationToken = default)
    {
        return await TransitionAsync(quoteId, q => q.Approve(), "ApproveClinicalQuoteCommand", OutboxPayloadFactory.ApproveClinicalQuote(quoteId, Guid.NewGuid()), cancellationToken);
    }

    public async Task<Result> RejectAsync(Guid quoteId, CancellationToken cancellationToken = default)
    {
        return await TransitionAsync(quoteId, q => q.Reject(), "RejectClinicalQuoteCommand", OutboxPayloadFactory.RejectClinicalQuote(quoteId, Guid.NewGuid()), cancellationToken);
    }

    public async Task<Result<IReadOnlyList<PendingQuoteConversionListItemDto>>> ListPendingConversionsAsync(CancellationToken cancellationToken = default)
    {
        var quotes = await _dbContext.ClinicalQuotes.AsNoTracking()
            .Include(q => q.Items)
            .Where(q => q.ConversionStatus == QuoteConversionStatus.Pending)
            .ToListAsync(cancellationToken);

        var result = new List<PendingQuoteConversionListItemDto>();
        foreach (var quote in quotes.OrderByDescending(q => q.DecidedAt))
        {
            var pet = await _dbContext.Pets.AsNoTracking().FirstOrDefaultAsync(p => p.Id == quote.PetId, cancellationToken);
            result.Add(new PendingQuoteConversionListItemDto
            {
                QuoteId = quote.Id,
                PetId = quote.PetId,
                PetName = pet?.Name ?? string.Empty,
                TotalAmount = quote.TotalAmount,
                DecidedAt = quote.DecidedAt
            });
        }

        return Result.Success<IReadOnlyList<PendingQuoteConversionListItemDto>>(result);
    }

    private async Task<Result> TransitionAsync(
        Guid quoteId,
        Func<ClinicalQuote, Result> transition,
        string outboxType,
        string payload,
        CancellationToken cancellationToken)
    {
        var quote = await _dbContext.ClinicalQuotes.FirstOrDefaultAsync(q => q.Id == quoteId, cancellationToken);
        if (quote is null)
        {
            return Result.Failure(new Error("ClinicalQuote.NotFound", "Quote not found locally."));
        }

        var result = transition(quote);
        if (result.IsFailure)
        {
            return result;
        }

        EnqueueOutbox(outboxType, payload);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private void EnqueueOutbox(string type, string payload)
    {
        var id = Guid.NewGuid();
        _dbContext.OutboxMessages.Add(new OutboxMessage
        {
            Id = id,
            Type = type,
            Payload = payload
        });
    }

    private static ClinicalQuoteListItemDto MapListItem(ClinicalQuote quote) =>
        new()
        {
            Id = quote.Id,
            Status = quote.Status.ToString(),
            ConversionStatus = quote.ConversionStatus.ToString(),
            TotalAmount = quote.TotalAmount,
            ItemCount = quote.Items.Count
        };

    private static ClinicalQuoteDetailDto MapDetail(ClinicalQuote quote) =>
        new()
        {
            Id = quote.Id,
            AppointmentId = quote.AppointmentId,
            PetId = quote.PetId,
            Status = quote.Status.ToString(),
            Notes = quote.Notes,
            TotalAmount = quote.TotalAmount,
            Items = quote.Items.OrderBy(i => i.SortOrder).Select(i => new ClinicalQuoteLineDto
            {
                Id = i.Id,
                Description = i.Description,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Kind = i.Kind.ToString(),
                SortOrder = i.SortOrder
            }).ToList()
        };
}
