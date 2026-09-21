using Clients.Infrastructure.Sales;
using Core.Domain;
using Microsoft.EntityFrameworkCore;
using Petshop.Domain.Entities;
using Petshop.Domain.Enums;

namespace Clients.Infrastructure.Crm;

/// <summary>
/// Offline grooming store backed by SQLite; debits local stock optimistically on complete.
/// </summary>
public sealed class OfflineGroomingStore : IGroomingStore
{
    private readonly OfflineDbContext _dbContext;
    private readonly IPetRepository _petRepository;

    public OfflineGroomingStore(OfflineDbContext dbContext, IPetRepository petRepository)
    {
        _dbContext = dbContext;
        _petRepository = petRepository;
    }

    public async Task<Result<List<GroomingAppointmentListItemDto>>> GetDailyAsync(
        Guid? groomerId,
        DateTimeOffset date,
        CancellationToken cancellationToken = default)
    {
        var (dayStart, dayEnd) = GetDayRange(date);
        var query = _dbContext.GroomingAppointments.AsQueryable();
        if (groomerId.HasValue)
        {
            query = query.Where(a => a.GroomerId == groomerId.Value);
        }

        var appointments = await query
            .Where(a => a.Date >= dayStart && a.Date < dayEnd)
            .OrderBy(a => a.Date)
            .ToListAsync(cancellationToken);

        var pets = await _petRepository.GetAllAsync(cancellationToken);
        var petNames = pets.ToDictionary(p => p.Id, p => p.Name);

        return Result.Success(appointments.Select(a => new GroomingAppointmentListItemDto
        {
            Id = a.Id,
            PetId = a.PetId,
            GroomerId = a.GroomerId,
            Date = a.Date,
            Notes = a.Notes,
            Status = a.Status.ToString(),
            PetName = petNames.TryGetValue(a.PetId, out var name) ? name : a.PetId.ToString()
        }).ToList());
    }

    public Task<Result> ConfirmAsync(Guid appointmentId, CancellationToken cancellationToken = default)
        => ApplyTransitionAsync(appointmentId, a => a.Confirm(), cancellationToken);

    public Task<Result> StartAsync(Guid appointmentId, CancellationToken cancellationToken = default)
        => ApplyTransitionAsync(appointmentId, a => a.Start(), cancellationToken);

    public Task<Result> MarkReadyAsync(Guid appointmentId, CancellationToken cancellationToken = default)
        => ApplyTransitionAsync(appointmentId, a => a.MarkReady(), cancellationToken);

    public async Task<Result> CompleteAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        var appointment = await _dbContext.GroomingAppointments.FindAsync([appointmentId], cancellationToken);
        if (appointment is null)
        {
            return Result.Failure(new Error("GroomingAppointment.NotFound", "Grooming appointment not found locally."));
        }

        var record = await _dbContext.GroomingRecords
            .Include(r => r.SupplyLines)
            .FirstOrDefaultAsync(r => r.GroomingAppointmentId == appointmentId, cancellationToken);

        if (record is not null)
        {
            var lines = record.SupplyLines.Select(l => (l.ProductId, l.Quantity)).ToList();
            var debit = await OfflineLocalStockSaleDebiter.DebitAsync(_dbContext, lines, cancellationToken);
            if (debit.IsFailure)
            {
                return debit;
            }

            record.FinalizeRecord();
        }

        var complete = appointment.Complete();
        if (complete.IsFailure)
        {
            return complete;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<GroomingHistoryItemDto>>> GetPetHistoryAsync(Guid petId, CancellationToken cancellationToken = default)
    {
        var records = await _dbContext.GroomingRecords
            .Where(r => r.PetId == petId)
            .OrderByDescending(r => r.UpdatedAt)
            .ToListAsync(cancellationToken);

        var items = new List<GroomingHistoryItemDto>();
        foreach (var record in records)
        {
            var appointment = await _dbContext.GroomingAppointments.FindAsync([record.GroomingAppointmentId], cancellationToken);
            items.Add(new GroomingHistoryItemDto
            {
                GroomingAppointmentId = record.GroomingAppointmentId,
                Date = appointment?.Date ?? record.UpdatedAt,
                CoatNotes = record.CoatNotes,
                Status = record.Status.ToString()
            });
        }

        return Result.Success<IReadOnlyList<GroomingHistoryItemDto>>(items);
    }

    private async Task<Result> ApplyTransitionAsync(
        Guid appointmentId,
        Func<GroomingAppointment, Result> transition,
        CancellationToken cancellationToken)
    {
        var appointment = await _dbContext.GroomingAppointments.FindAsync([appointmentId], cancellationToken);
        if (appointment is null)
        {
            return Result.Failure(new Error("GroomingAppointment.NotFound", "Grooming appointment not found locally."));
        }

        var result = transition(appointment);
        if (result.IsFailure)
        {
            return result;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static (DateTimeOffset Start, DateTimeOffset End) GetDayRange(DateTimeOffset date)
    {
        var utcDay = DateTime.SpecifyKind(date.UtcDateTime.Date, DateTimeKind.Utc);
        var start = new DateTimeOffset(utcDay, TimeSpan.Zero);
        return (start, start.AddDays(1));
    }
}
