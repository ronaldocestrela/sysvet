using Microsoft.EntityFrameworkCore;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Infrastructure.Persistence.Repositories;

public sealed class PrescriptionTemplateRepository : IPrescriptionTemplateRepository
{
    private readonly VeterinaryDbContext _dbContext;

    public PrescriptionTemplateRepository(VeterinaryDbContext dbContext) => _dbContext = dbContext;

    public async Task AddAsync(PrescriptionTemplate template, CancellationToken cancellationToken = default) =>
        await _dbContext.PrescriptionTemplates.AddAsync(template, cancellationToken);

    public async Task<PrescriptionTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.PrescriptionTemplates.Include(t => t.Items).FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<PrescriptionTemplate>> ListActiveAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.PrescriptionTemplates.AsNoTracking()
            .Include(t => t.Items)
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

    public void Update(PrescriptionTemplate template) => _dbContext.PrescriptionTemplates.Update(template);
}
