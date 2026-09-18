using Microsoft.EntityFrameworkCore;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Infrastructure.Persistence.Repositories;

public class MedicalRecordRepository : IMedicalRecordRepository
{
    private readonly VeterinaryDbContext _dbContext;

    public MedicalRecordRepository(VeterinaryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(MedicalRecord medicalRecord, CancellationToken cancellationToken = default)
    {
        await _dbContext.MedicalRecords.AddAsync(medicalRecord, cancellationToken);
    }

    public async Task<MedicalRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.MedicalRecords
            .Include(m => m.EvolutionNotes)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<MedicalRecord?> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.MedicalRecords
            .Include(m => m.EvolutionNotes)
            .FirstOrDefaultAsync(m => m.AppointmentId == appointmentId, cancellationToken);
    }

    public async Task<IReadOnlyList<MedicalRecord>> GetByPetIdAsync(Guid petId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.MedicalRecords
            .AsNoTracking()
            .Where(m => m.PetId == petId)
            .OrderByDescending(m => m.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public void Update(MedicalRecord medicalRecord)
    {
        _dbContext.MedicalRecords.Update(medicalRecord);
    }
}
