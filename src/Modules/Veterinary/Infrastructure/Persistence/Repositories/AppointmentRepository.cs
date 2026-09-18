using Core.Domain;
using Microsoft.EntityFrameworkCore;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Enums;
using Veterinary.Domain.Repositories;

namespace Veterinary.Infrastructure.Persistence.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly VeterinaryDbContext _dbContext;

    public AppointmentRepository(VeterinaryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(Appointment entity) => _dbContext.Appointments.Add(entity);

    public async Task AddAsync(Appointment entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Appointments.AddAsync(entity, cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Appointments.ToListAsync(cancellationToken);

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.Appointments.FindAsync(new object[] { id }, cancellationToken);

    public void Remove(Appointment entity) => _dbContext.Appointments.Remove(entity);

    public void Update(Appointment entity) => _dbContext.Appointments.Update(entity);

    public async Task<IEnumerable<Appointment>> GetByVeterinarianAndDateAsync(Guid veterinarianId, DateTimeOffset date, CancellationToken cancellationToken = default)
        => await GetByDayAsync(veterinarianId, date, cancellationToken);

    public async Task<IEnumerable<Appointment>> GetByDayAsync(Guid? veterinarianId, DateTimeOffset date, CancellationToken cancellationToken = default)
    {
        var (dayStart, dayEnd) = GetDayRange(date);
        var query = _dbContext.Appointments.AsQueryable();

        if (veterinarianId.HasValue)
        {
            query = query.Where(a => a.VeterinarianId == veterinarianId.Value);
        }

        var candidates = await query
            .Where(a => a.Date >= dayStart && a.Date < dayEnd)
            .ToListAsync(cancellationToken);

        return candidates;
    }

    public async Task<bool> HasOverlappingAsync(
        Guid veterinarianId,
        DateTimeOffset start,
        int durationMinutes,
        Guid? excludeAppointmentId,
        CancellationToken cancellationToken = default)
    {
        var end = start.AddMinutes(durationMinutes);
        var activeStatuses = new[]
        {
            AppointmentStatus.Scheduled,
            AppointmentStatus.Confirmed,
            AppointmentStatus.InProgress
        };

        var appointments = await _dbContext.Appointments
            .Where(a => a.VeterinarianId == veterinarianId && activeStatuses.Contains(a.Status))
            .ToListAsync(cancellationToken);

        if (excludeAppointmentId.HasValue)
        {
            appointments = appointments.Where(a => a.Id != excludeAppointmentId.Value).ToList();
        }

        return appointments.Any(a => a.Overlaps(start, end));
    }

    private static (DateTimeOffset Start, DateTimeOffset End) GetDayRange(DateTimeOffset date)
    {
        var utcDay = DateTime.SpecifyKind(date.UtcDateTime.Date, DateTimeKind.Utc);
        var start = new DateTimeOffset(utcDay, TimeSpan.Zero);
        return (start, start.AddDays(1));
    }
}
