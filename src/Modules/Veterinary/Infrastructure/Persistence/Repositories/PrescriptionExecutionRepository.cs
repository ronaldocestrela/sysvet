using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Infrastructure.Persistence.Repositories;

public class PrescriptionExecutionRepository : IPrescriptionExecutionRepository
{
    private readonly VeterinaryDbContext _dbContext;

    public PrescriptionExecutionRepository(VeterinaryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(PrescriptionExecution prescriptionExecution, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<PrescriptionExecution>().AddAsync(prescriptionExecution, cancellationToken);
    }

    public async Task<List<PrescriptionExecution>> GetByHospitalizationIdAsync(Guid hospitalizationId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<PrescriptionExecution>()
            .Where(p => p.HospitalizationId == hospitalizationId)
            .OrderByDescending(p => p.ExecutedAt)
            .ToListAsync(cancellationToken);
    }
}
