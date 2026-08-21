using Core.Domain;
using Microsoft.EntityFrameworkCore;
using Veterinary.Domain.Entities;
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
    {
        return await _dbContext.Appointments
            .Where(a => a.VeterinarianId == veterinarianId && a.Date.Date == date.Date)
            .ToListAsync(cancellationToken);
    }
}
