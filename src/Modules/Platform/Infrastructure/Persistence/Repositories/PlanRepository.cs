using Microsoft.EntityFrameworkCore;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Persistence.Repositories;

/// <inheritdoc />
public sealed class PlanRepository : IPlanRepository
{
    private readonly PlatformDbContext _context;

    /// <summary>Creates the repository.</summary>
    public PlanRepository(PlatformDbContext context) => _context = context;

    /// <inheritdoc />
    public Task<IReadOnlyList<Plan>> ListAsync(CancellationToken cancellationToken = default) =>
        _context.Plans.Include(p => p.IncludedModules).OrderBy(p => p.Code).ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<Plan>)t.Result, cancellationToken);

    /// <inheritdoc />
    public Task<Plan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Plans.Include(p => p.IncludedModules).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    /// <inheritdoc />
    public Task<Plan?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        _context.Plans.Include(p => p.IncludedModules)
            .FirstOrDefaultAsync(p => p.Code == code, cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(Plan plan, CancellationToken cancellationToken = default) =>
        await _context.Plans.AddAsync(plan, cancellationToken);

    /// <inheritdoc />
    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default) =>
        _context.Plans.AnyAsync(p => p.Code == code, cancellationToken);
}
