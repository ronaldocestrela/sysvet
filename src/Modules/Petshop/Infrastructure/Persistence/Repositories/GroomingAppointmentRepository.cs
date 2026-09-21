using Microsoft.EntityFrameworkCore;
using Petshop.Domain.Entities;
using Petshop.Domain.Enums;
using Petshop.Domain.Repositories;

namespace Petshop.Infrastructure.Persistence.Repositories;

public sealed class GroomingAppointmentRepository : IGroomingAppointmentRepository
{
    private readonly PetshopDbContext _dbContext;

    public GroomingAppointmentRepository(PetshopDbContext dbContext) => _dbContext = dbContext;

    public void Add(GroomingAppointment entity) => _dbContext.GroomingAppointments.Add(entity);

    public async Task AddAsync(GroomingAppointment entity, CancellationToken cancellationToken = default)
        => await _dbContext.GroomingAppointments.AddAsync(entity, cancellationToken);

    public async Task<IEnumerable<GroomingAppointment>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.GroomingAppointments.ToListAsync(cancellationToken);

    public async Task<GroomingAppointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.GroomingAppointments.FindAsync([id], cancellationToken);

    public void Remove(GroomingAppointment entity) => _dbContext.GroomingAppointments.Remove(entity);

    public void Update(GroomingAppointment entity) => _dbContext.GroomingAppointments.Update(entity);

    public async Task<IEnumerable<GroomingAppointment>> GetByDayAsync(Guid? groomerId, DateTimeOffset date, CancellationToken cancellationToken = default)
    {
        var (dayStart, dayEnd) = GetDayRange(date);
        var query = _dbContext.GroomingAppointments.AsQueryable();
        if (groomerId.HasValue)
        {
            query = query.Where(a => a.GroomerId == groomerId.Value);
        }

        var candidates = await query.Where(a => a.Date >= dayStart && a.Date < dayEnd).ToListAsync(cancellationToken);
        return candidates;
    }

    public async Task<bool> HasOverlappingAsync(
        Guid groomerId,
        DateTimeOffset start,
        int durationMinutes,
        Guid? excludeAppointmentId,
        CancellationToken cancellationToken = default)
    {
        var end = start.AddMinutes(durationMinutes);
        var activeStatuses = new[]
        {
            GroomingAppointmentStatus.Scheduled,
            GroomingAppointmentStatus.Confirmed,
            GroomingAppointmentStatus.InProgress
        };

        var appointments = await _dbContext.GroomingAppointments
            .Where(a => a.GroomerId == groomerId && activeStatuses.Contains(a.Status))
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
