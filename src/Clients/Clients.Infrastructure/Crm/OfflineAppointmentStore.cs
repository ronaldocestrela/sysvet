using Core.Domain;
using Microsoft.EntityFrameworkCore;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Enums;

namespace Clients.Infrastructure.Crm;

/// <summary>
/// Offline agenda store backed by SQLite and transactional outbox.
/// </summary>
public sealed class OfflineAppointmentStore : IAppointmentStore
{
    private readonly OfflineDbContext _dbContext;
    private readonly IPetRepository _petRepository;

    public OfflineAppointmentStore(OfflineDbContext dbContext, IPetRepository petRepository)
    {
        _dbContext = dbContext;
        _petRepository = petRepository;
    }

    /// <inheritdoc />
    public async Task<Result<List<AppointmentListItemDto>>> GetDailyAsync(
        Guid? veterinarianId,
        DateTimeOffset date,
        CancellationToken cancellationToken = default)
    {
        var (dayStart, dayEnd) = GetDayRange(date);
        var query = _dbContext.Appointments.AsQueryable();
        if (veterinarianId.HasValue)
        {
            query = query.Where(a => a.VeterinarianId == veterinarianId.Value);
        }

        var appointments = await query
            .Where(a => a.Date >= dayStart && a.Date < dayEnd)
            .OrderBy(a => a.Date)
            .ToListAsync(cancellationToken);

        var pets = await _petRepository.GetAllAsync(cancellationToken);
        var petNames = pets.ToDictionary(p => p.Id, p => p.Name);

        var dtos = appointments.Select(a => new AppointmentListItemDto
        {
            Id = a.Id,
            PetId = a.PetId,
            VeterinarianId = a.VeterinarianId,
            Date = a.Date,
            Reason = a.Reason,
            Status = a.Status.ToString(),
            PetName = petNames.TryGetValue(a.PetId, out var name) ? name : a.PetId.ToString()
        }).ToList();

        return Result.Success(dtos);
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> ScheduleAsync(ScheduleAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Appointments.FindAsync([request.Id], cancellationToken);
        if (existing is not null)
        {
            return Result.Success(existing.Id);
        }

        var appointmentResult = Appointment.Create(
            request.Id,
            request.TutorId,
            request.PetId,
            request.VeterinarianId,
            request.Date,
            request.DurationInMinutes,
            request.Reason);

        if (appointmentResult.IsFailure)
        {
            return Result.Failure<Guid>(appointmentResult.Error);
        }

        var slots = await _dbContext.ScheduleSlots
            .Where(s => s.VeterinarianId == request.VeterinarianId && s.IsAvailable)
            .ToListAsync(cancellationToken);

        var slot = FindCoveringSlot(slots, request.Date, request.DurationInMinutes);
        slot?.Book();

        _dbContext.Appointments.Add(appointmentResult.Value);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(appointmentResult.Value.Id);
    }

    /// <inheritdoc />
    public async Task<Result> ConfirmAsync(Guid appointmentId, CancellationToken cancellationToken = default)
        => await ApplyTransitionAsync(appointmentId, a => a.Confirm(), cancellationToken);

    /// <inheritdoc />
    public async Task<Result> StartAsync(Guid appointmentId, CancellationToken cancellationToken = default)
        => await ApplyTransitionAsync(appointmentId, a => a.Start(), cancellationToken);

    /// <inheritdoc />
    public async Task<Result> CancelAsync(Guid appointmentId, CancellationToken cancellationToken = default)
        => await ApplyTransitionAsync(appointmentId, a => a.Cancel(), cancellationToken);

    private async Task<Result> ApplyTransitionAsync(
        Guid appointmentId,
        Func<Appointment, Result> transition,
        CancellationToken cancellationToken)
    {
        var appointment = await _dbContext.Appointments.FindAsync([appointmentId], cancellationToken);
        if (appointment is null)
        {
            return Result.Failure(new Error("Appointment.NotFound", "Appointment not found locally."));
        }

        var result = transition(appointment);
        if (result.IsFailure)
        {
            return result;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static ScheduleSlot? FindCoveringSlot(IEnumerable<ScheduleSlot> slots, DateTimeOffset date, int durationMinutes)
    {
        var startTime = date.TimeOfDay;
        var endTime = startTime.Add(TimeSpan.FromMinutes(durationMinutes));
        return slots.FirstOrDefault(s => s.StartTime <= startTime && s.EndTime >= endTime);
    }

    private static (DateTimeOffset Start, DateTimeOffset End) GetDayRange(DateTimeOffset date)
    {
        var utcDay = DateTime.SpecifyKind(date.UtcDateTime.Date, DateTimeKind.Utc);
        var start = new DateTimeOffset(utcDay, TimeSpan.Zero);
        return (start, start.AddDays(1));
    }
}
