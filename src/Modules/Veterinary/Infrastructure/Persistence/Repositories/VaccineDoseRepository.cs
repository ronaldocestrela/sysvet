using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Infrastructure.Persistence.Repositories;

public class VaccineDoseRepository : IVaccineDoseRepository
{
    private readonly VeterinaryDbContext _dbContext;

    public VaccineDoseRepository(VeterinaryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(VaccineDose vaccineDose, CancellationToken cancellationToken = default)
    {
        await _dbContext.VaccineDoses.AddAsync(vaccineDose, cancellationToken);
    }

    public async Task<VaccineDose?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.VaccineDoses.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<List<VaccineDose>> GetByPetIdAsync(Guid petId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.VaccineDoses
            .Where(v => v.PetId == petId)
            .OrderByDescending(v => v.AppliedAt)
            .ToListAsync(cancellationToken);
    }

    public void Update(VaccineDose vaccineDose)
    {
        _dbContext.VaccineDoses.Update(vaccineDose);
    }
}
