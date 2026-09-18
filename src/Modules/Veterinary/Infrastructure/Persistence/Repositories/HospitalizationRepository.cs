using Core.Domain;
using Microsoft.EntityFrameworkCore;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Infrastructure.Persistence.Repositories;

public class HospitalizationRepository : IHospitalizationRepository
{
    private readonly VeterinaryDbContext _dbContext;

    public HospitalizationRepository(VeterinaryDbContext dbContext) => _dbContext = dbContext;

    public async Task AddAsync(Hospitalization hospitalization, CancellationToken cancellationToken = default) =>
        await _dbContext.Hospitalizations.AddAsync(hospitalization, cancellationToken);

    public async Task<Hospitalization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Hospitalizations
            .Include(h => h.MedicationOrders)
            .Include(h => h.Administrations)
            .Include(h => h.ProgressNotes)
            .Include(h => h.Procedures)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

    public async Task<List<Hospitalization>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var items = await _dbContext.Hospitalizations
            .AsNoTracking()
            .Include(h => h.MedicationOrders)
            .Include(h => h.Administrations)
            .Where(h => h.Status == HospitalizationStatus.Admitted)
            .ToListAsync(cancellationToken);

        return items.OrderBy(h => h.AdmittedAt).ToList();
    }

    public async Task<Hospitalization?> GetActiveByPetAsync(Guid petId, CancellationToken cancellationToken = default) =>
        await _dbContext.Hospitalizations
            .FirstOrDefaultAsync(h => h.PetId == petId && h.Status == HospitalizationStatus.Admitted, cancellationToken);

    public async Task<Hospitalization?> GetActiveByBedAsync(Guid bedId, CancellationToken cancellationToken = default) =>
        await _dbContext.Hospitalizations
            .FirstOrDefaultAsync(h => h.BedId == bedId && h.Status == HospitalizationStatus.Admitted, cancellationToken);

    public void Update(Hospitalization hospitalization)
    {
        var entry = _dbContext.Entry(hospitalization);
        if (entry.State == EntityState.Detached)
        {
            _dbContext.Hospitalizations.Attach(hospitalization);
            entry.State = EntityState.Unchanged;
        }

        StageNewChildren(hospitalization.MedicationOrders, _dbContext.HospitalMedicationOrders);
        StageNewChildren(hospitalization.Administrations, _dbContext.MedicationAdministrations);
        StageNewChildren(hospitalization.ProgressNotes, _dbContext.HospitalizationProgressNotes);
        StageNewChildren(hospitalization.Procedures, _dbContext.HospitalProcedures);
    }

    private void StageNewChildren<TEntity>(IEnumerable<TEntity> children, DbSet<TEntity> set)
        where TEntity : Entity
    {
        foreach (var child in children)
        {
            var childEntry = _dbContext.Entry(child);
            if (childEntry.State == EntityState.Added)
            {
                continue;
            }

            var existsInDatabase = set.AsNoTracking().Any(e => e.Id == child.Id);
            if (existsInDatabase)
            {
                continue;
            }

            childEntry.State = EntityState.Added;
        }
    }
}
