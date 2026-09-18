using Microsoft.EntityFrameworkCore;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Infrastructure.Persistence.Repositories;

public sealed class ClinicalExamRepository : IClinicalExamRepository
{
    private readonly VeterinaryDbContext _dbContext;

    public ClinicalExamRepository(VeterinaryDbContext dbContext) => _dbContext = dbContext;

    public async Task AddAsync(ClinicalExam exam, CancellationToken cancellationToken = default) =>
        await _dbContext.ClinicalExams.AddAsync(exam, cancellationToken);

    public async Task<ClinicalExam?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.ClinicalExams.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ClinicalExam>> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        var exams = await _dbContext.ClinicalExams.AsNoTracking()
            .Where(e => e.AppointmentId == appointmentId)
            .ToListAsync(cancellationToken);
        return exams.OrderByDescending(e => e.UpdatedAt).ToList();
    }

    public async Task<IReadOnlyList<ClinicalExam>> GetByPetIdAsync(Guid petId, CancellationToken cancellationToken = default)
    {
        var exams = await _dbContext.ClinicalExams.AsNoTracking()
            .Where(e => e.PetId == petId)
            .ToListAsync(cancellationToken);
        return exams.OrderByDescending(e => e.UpdatedAt).ToList();
    }

    public void Update(ClinicalExam exam) => _dbContext.ClinicalExams.Update(exam);
}
