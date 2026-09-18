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
        var doses = await _dbContext.VaccineDoses
            .Where(v => v.PetId == petId)
            .ToListAsync(cancellationToken);
        return doses.OrderByDescending(v => v.AppliedAt).ToList();
    }

    public async Task<List<VaccineDose>> GetDueAsync(DateTimeOffset utcNow, DateTimeOffset until, int take, CancellationToken cancellationToken = default)
    {
        var doses = await _dbContext.VaccineDoses
            .Where(v => v.NextDueDate != null)
            .ToListAsync(cancellationToken);
        return doses
            .Where(v => v.NextDueDate >= utcNow && v.NextDueDate <= until)
            .OrderBy(v => v.NextDueDate)
            .Take(take)
            .ToList();
    }

    public async Task<List<VaccineDose>> GetOverdueAsync(DateTimeOffset utcNow, int take, CancellationToken cancellationToken = default)
    {
        var doses = await _dbContext.VaccineDoses
            .Where(v => v.NextDueDate != null)
            .ToListAsync(cancellationToken);
        return doses
            .Where(v => v.NextDueDate < utcNow)
            .OrderBy(v => v.NextDueDate)
            .Take(take)
            .ToList();
    }

    public void Update(VaccineDose vaccineDose)
    {
        _dbContext.VaccineDoses.Update(vaccineDose);
    }
}
