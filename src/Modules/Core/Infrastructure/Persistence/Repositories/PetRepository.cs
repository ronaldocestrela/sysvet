using Core.Domain;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.Infrastructure.Persistence.Repositories;

public class PetRepository : Repository<Pet>, IPetRepository
{
    public PetRepository(CoreDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IEnumerable<Pet>> GetByTutorIdAsync(Guid tutorId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Pets
            .Where(p => p.TutorId == tutorId)
            .ToListAsync(cancellationToken);
    }
}
