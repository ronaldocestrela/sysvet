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

    /// <inheritdoc />
    public async Task<PagedList<Tutor>> SearchAsync(
        int page,
        int pageSize,
        string? nameFilter,
        string? cpfFilter,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Tutors.AsQueryable();

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            query = query.Where(t => EF.Functions.Like(t.Name, $"%{nameFilter.Trim()}%"));
        }

        if (!string.IsNullOrWhiteSpace(cpfFilter))
        {
            var cpf = cpfFilter.Trim();
            query = query.Where(t => t.Cpf.Number.Contains(cpf));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<Tutor>(items, total);
    }
}
