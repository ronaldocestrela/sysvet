using System;
using System.Threading;
using System.Threading.Tasks;
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
        return await _dbContext.MedicalRecords.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public void Update(MedicalRecord medicalRecord)
    {
        _dbContext.MedicalRecords.Update(medicalRecord);
    }
}
