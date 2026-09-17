using Core.Domain;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clients.Infrastructure.Persistence.Repositories;

/// <summary>
/// SQLite implementation of <see cref="ITutorRepository"/> on the client.
/// </summary>
public sealed class OfflineTutorRepository : OfflineRepositoryBase<Tutor>, ITutorRepository
{
    /// <summary>Creates the repository.</summary>
    public OfflineTutorRepository(OfflineDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc />
    public Task<Tutor?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default)
        => DbContext.Tutors.FirstOrDefaultAsync(t => t.Cpf.Number == cpf, cancellationToken);

    /// <inheritdoc />
    public Task<Tutor?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => DbContext.Tutors.FirstOrDefaultAsync(t => t.Email.Address == email, cancellationToken);

    /// <inheritdoc />
    public async Task<PagedList<Tutor>> SearchAsync(
        int page,
        int pageSize,
        string? nameFilter,
        string? cpfFilter,
        CancellationToken cancellationToken = default)
    {
        var query = DbContext.Tutors.AsQueryable();

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
