using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Infrastructure.Persistence.Repositories;

public class HospitalizationRepository : IHospitalizationRepository
{
    private readonly VeterinaryDbContext _dbContext;

    public HospitalizationRepository(VeterinaryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Hospitalization hospitalization, CancellationToken cancellationToken = default)
    {
        await _dbContext.Hospitalizations.AddAsync(hospitalization, cancellationToken);
    }

    public async Task<Hospitalization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Hospitalizations
            .Include(h => h.PrescriptionExecutions)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public void Update(Hospitalization hospitalization)
    {
        _dbContext.Hospitalizations.Update(hospitalization);
    }
}
