using Core.Domain;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.Infrastructure.Persistence.Repositories;

public class TutorRepository : Repository<Tutor>, ITutorRepository
{
    public TutorRepository(CoreDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Tutor?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tutors.FirstOrDefaultAsync(t => t.Cpf.Number == cpf, cancellationToken);
    }

    public async Task<Tutor?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tutors.FirstOrDefaultAsync(t => t.Email.Address == email, cancellationToken);
    }
}
